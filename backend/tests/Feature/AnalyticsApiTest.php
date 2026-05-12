<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Enums\OrderStatus;
use App\Models\Order;
use App\Models\OrderItem;
use App\Models\Reservation;
use App\Models\Restaurant;
use App\Models\Table;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Carbon;
use Tests\TestCase;

class AnalyticsApiTest extends TestCase
{
    use RefreshDatabase;

    private Restaurant $restaurant;
    private User $admin;
    private User $waiter;
    private User $cashier;
    private Table $table;

    protected function setUp(): void
    {
        parent::setUp();

        $this->restaurant = Restaurant::factory()->create();

        $this->admin = User::factory()->create([
            'role'          => 'admin',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->waiter = User::factory()->create([
            'role'          => 'waiter',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->cashier = User::factory()->create([
            'role'          => 'cashier',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->table = Table::factory()->create([
            'restaurant_id' => $this->restaurant->id,
        ]);
    }

    public function test_admin_can_get_analytics_summary(): void
    {
        $response = $this->actingAs($this->admin)
            ->getJson('/api/analytics/summary')
            ->assertOk();

        $response->assertJsonStructure([
            'revenue' => ['today', 'this_week', 'this_month'],
            'orders'  => ['today', 'this_week', 'this_month', 'average_value'],
            'top_items',
            'reservations' => ['today', 'this_week'],
        ]);
    }

    public function test_waiter_cannot_get_analytics_summary(): void
    {
        $this->actingAs($this->waiter)
            ->getJson('/api/analytics/summary')
            ->assertForbidden();
    }

    public function test_cashier_cannot_get_analytics_summary(): void
    {
        $this->actingAs($this->cashier)
            ->getJson('/api/analytics/summary')
            ->assertForbidden();
    }

    public function test_unauthenticated_cannot_get_analytics_summary(): void
    {
        $this->getJson('/api/analytics/summary')
            ->assertUnauthorized();
    }

    public function test_revenue_reflects_paid_orders_today(): void
    {
        Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id'      => $this->table->id,
            'user_id'       => $this->waiter->id,
            'status'        => OrderStatus::PAID,
            'total_amount'  => 500.00,
            'paid_at'       => Carbon::today(),
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson('/api/analytics/summary')
            ->assertOk();

        $this->assertEquals(500.00, $response->json('revenue.today'));
    }

    public function test_revenue_excludes_pending_orders(): void
    {
        Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id'      => $this->table->id,
            'user_id'       => $this->waiter->id,
            'status'        => OrderStatus::PENDING,
            'total_amount'  => 300.00,
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson('/api/analytics/summary')
            ->assertOk();

        $this->assertEquals(0.0, $response->json('revenue.today'));
    }

    public function test_top_items_returns_up_to_five_entries(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id'      => $this->table->id,
            'user_id'       => $this->waiter->id,
            'status'        => OrderStatus::PAID,
            'paid_at'       => Carbon::today(),
        ]);

        for ($i = 1; $i <= 6; $i++) {
            OrderItem::factory()->create([
                'order_id' => $order->id,
                'name'     => "Item {$i}",
                'quantity' => $i,
                'price'    => 100.00,
                'status'   => 'ready',
            ]);
        }

        $response = $this->actingAs($this->admin)
            ->getJson('/api/analytics/summary')
            ->assertOk();

        $this->assertCount(5, $response->json('top_items'));
    }

    public function test_top_items_structure(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id'      => $this->table->id,
            'user_id'       => $this->waiter->id,
            'status'        => OrderStatus::PAID,
            'paid_at'       => Carbon::today(),
        ]);

        OrderItem::factory()->create([
            'order_id' => $order->id,
            'name'     => 'Burger',
            'quantity' => 3,
            'price'    => 200.00,
            'status'   => 'ready',
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson('/api/analytics/summary')
            ->assertOk();

        $response->assertJsonStructure([
            'top_items' => [
                '*' => ['name', 'quantity_sold', 'revenue'],
            ],
        ]);

        $item = $response->json('top_items.0');
        $this->assertEquals('Burger', $item['name']);
        $this->assertEquals(3, $item['quantity_sold']);
        $this->assertEquals(600.00, $item['revenue']);
    }

    public function test_reservations_count_today(): void
    {
        Reservation::factory()->count(2)->create([
            'restaurant_id'    => $this->restaurant->id,
            'reservation_time' => Carbon::today()->addHours(12),
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson('/api/analytics/summary')
            ->assertOk();

        $this->assertEquals(2, $response->json('reservations.today'));
    }
}
