<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Enums\OrderItemStatus;
use App\Events\OrderBillingRequested;
use App\Models\MenuItem;
use App\Models\Order;
use App\Models\OrderItem;
use App\Models\Reservation;
use App\Models\Restaurant;
use App\Models\Table;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Event;
use Tests\TestCase;

class OrderApiTest extends TestCase
{
    use RefreshDatabase;

    private Restaurant $restaurant;

    private User $waiter;

    private User $cashier;

    private User $chef;

    private User $guest;

    private Table $table;

    private MenuItem $menuItem;

    protected function setUp(): void
    {
        parent::setUp();

        $this->restaurant = Restaurant::factory()->create();

        $this->waiter = User::factory()->create([
            'role' => 'waiter',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->cashier = User::factory()->create([
            'role' => 'cashier',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->chef = User::factory()->create([
            'role' => 'chef',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->guest = User::factory()->create(['role' => 'guest']);

        $this->table = Table::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'status' => 'free',
        ]);

        $this->menuItem = MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'price' => 150.00,
        ]);
    }


    public function test_complete_order_lifecycle(): void
    {
        $orderResponse = $this->actingAs($this->waiter)
            ->postJson('/api/orders', ['table_id' => $this->table->id])
            ->assertStatus(201);

        $orderId = $orderResponse->json('id');

        $this->actingAs($this->waiter)
            ->postJson("/api/orders/{$orderId}/items", [
                'items' => [['menu_item_id' => $this->menuItem->id, 'quantity' => 2]],
            ])
            ->assertStatus(201);

        $this->assertDatabaseHas('orders', ['id' => $orderId, 'total_amount' => 300.00]);

        $this->actingAs($this->waiter)
            ->patchJson("/api/orders/{$orderId}/pay")
            ->assertOk();

        $this->assertDatabaseHas('orders', ['id' => $orderId, 'status' => 'paid']);
        $this->assertDatabaseHas('tables', ['id' => $this->table->id, 'status' => 'free']);
        $this->assertDatabaseHas('audit_logs', ['user_id' => $this->waiter->id, 'action' => 'order.paid']);
    }


    public function test_waiter_can_list_orders(): void
    {
        Order::factory()->count(2)->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
        ]);

        $this->actingAs($this->waiter)
            ->getJson('/api/orders')
            ->assertOk()
            ->assertJsonCount(2);
    }

    public function test_guest_cannot_list_orders(): void
    {
        $this->actingAs($this->guest)
            ->getJson('/api/orders')
            ->assertForbidden();
    }


    public function test_waiter_can_view_order(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
        ]);

        $this->actingAs($this->waiter)
            ->getJson("/api/orders/{$order->id}")
            ->assertOk()
            ->assertJsonFragment(['id' => $order->id]);
    }


    public function test_waiter_can_remove_order_item(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
        ]);

        $item = OrderItem::factory()->create([
            'order_id' => $order->id,
            'menu_item_id' => $this->menuItem->id,
            'status' => OrderItemStatus::PENDING,
        ]);

        $this->actingAs($this->waiter)
            ->deleteJson("/api/orders/items/{$item->id}")
            ->assertNoContent();

        $this->assertDatabaseMissing('order_items', ['id' => $item->id]);
    }


    public function test_cashier_can_view_bill(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
            'total_amount' => 300.00,
        ]);

        OrderItem::factory()->create([
            'order_id' => $order->id,
            'menu_item_id' => $this->menuItem->id,
            'quantity' => 2,
            'price' => 150.00,
            'status' => OrderItemStatus::READY,
        ]);

        $response = $this->actingAs($this->cashier)
            ->getJson("/api/orders/{$order->id}/bill")
            ->assertOk();

        $response->assertJsonStructure([
            'order_id', 'table_number', 'status', 'total_amount', 'paid_at', 'items',
        ]);

        $this->assertEquals(300.00, $response->json('total_amount'));
        $this->assertEquals(2, $response->json('items.0.quantity'));
        $this->assertEquals(150.00, $response->json('items.0.unit_price'));
        $this->assertEquals(300.00, $response->json('items.0.subtotal'));
    }

    public function test_waiter_can_view_bill(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
        ]);

        $this->actingAs($this->waiter)
            ->getJson("/api/orders/{$order->id}/bill")
            ->assertOk();
    }

    public function test_guest_can_view_own_bill(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->guest->id,
        ]);

        $this->actingAs($this->guest)
            ->getJson("/api/orders/{$order->id}/bill")
            ->assertOk();
    }

    public function test_chef_cannot_view_bill(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
        ]);

        $this->actingAs($this->chef)
            ->getJson("/api/orders/{$order->id}/bill")
            ->assertOk();
    }


    public function test_cashier_can_pay_order(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
        ]);

        $this->actingAs($this->cashier)
            ->patchJson("/api/orders/{$order->id}/pay")
            ->assertOk();

        $this->assertDatabaseHas('orders', ['id' => $order->id, 'status' => 'paid']);
        $this->assertDatabaseHas('tables', ['id' => $this->table->id, 'status' => 'free']);
    }

    public function test_chef_cannot_pay_order(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
        ]);

        $this->actingAs($this->chef)
            ->patchJson("/api/orders/{$order->id}/pay")
            ->assertForbidden();
    }

    public function test_cannot_pay_already_paid_order(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->waiter->id,
            'status' => 'paid',
        ]);

        $this->actingAs($this->cashier)
            ->patchJson("/api/orders/{$order->id}/pay")
            ->assertForbidden();
    }


    public function test_request_bill_broadcasts_event(): void
    {
        Event::fake([OrderBillingRequested::class]);

        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->guest->id,
        ]);

        $this->actingAs($this->guest)
            ->patchJson("/api/orders/{$order->id}/request-bill")
            ->assertOk();

        Event::assertDispatched(OrderBillingRequested::class);
    }


    public function test_guest_can_create_order_with_confirmed_reservation(): void
    {
        $reservation = Reservation::factory()->confirmed()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->guest->id,
        ]);

        $this->actingAs($this->guest)
            ->postJson('/api/orders', [
                'table_id' => $this->table->id,
                'reservation_id' => $reservation->id,
            ])
            ->assertStatus(201);
    }

    public function test_guest_cannot_create_order_without_reservation(): void
    {
        $this->actingAs($this->guest)
            ->postJson('/api/orders', ['table_id' => $this->table->id])
            ->assertUnprocessable();
    }

    public function test_guest_cannot_create_order_with_someone_elses_reservation(): void
    {
        $otherGuest = User::factory()->create(['role' => 'guest']);

        $reservation = Reservation::factory()->confirmed()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $otherGuest->id,
        ]);

        $this->actingAs($this->guest)
            ->postJson('/api/orders', [
                'table_id' => $this->table->id,
                'reservation_id' => $reservation->id,
            ])
            ->assertUnprocessable();
    }
}
