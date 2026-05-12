<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Enums\OrderItemStatus;
use App\Events\OrderBillingRequested;
use App\Models\MenuItem;
use App\Models\Order;
use App\Models\OrderItem;
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
    private User $chef;
    private User $guest;
    private Table $table;
    private MenuItem $menuItem;

    protected function setUp(): void
    {
        parent::setUp();

        $this->restaurant = Restaurant::factory()->create();

        $this->waiter = User::factory()->create([
            'role'          => 'waiter',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->chef = User::factory()->create([
            'role'          => 'chef',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->guest = User::factory()->create(['role' => 'guest']);

        $this->table = Table::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'status'        => 'free',
        ]);

        $this->menuItem = MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'price'         => 150.00,
        ]);
    }

    // --- Lifecycle ---

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

    // --- Список заказов ---

    public function test_waiter_can_list_orders(): void
    {
        Order::factory()->count(2)->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id'      => $this->table->id,
            'user_id'       => $this->waiter->id,
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

    // --- Просмотр заказа ---

    public function test_waiter_can_view_order(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id'      => $this->table->id,
            'user_id'       => $this->waiter->id,
        ]);

        $this->actingAs($this->waiter)
            ->getJson("/api/orders/{$order->id}")
            ->assertOk()
            ->assertJsonFragment(['id' => $order->id]);
    }

    // --- Удаление позиции ---

    public function test_waiter_can_remove_order_item(): void
    {
        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id'      => $this->table->id,
            'user_id'       => $this->waiter->id,
        ]);

        $item = OrderItem::factory()->create([
            'order_id'     => $order->id,
            'menu_item_id' => $this->menuItem->id,
            'status'       => OrderItemStatus::PENDING,
        ]);

        $this->actingAs($this->waiter)
            ->deleteJson("/api/orders/items/{$item->id}")
            ->assertNoContent();

        $this->assertDatabaseMissing('order_items', ['id' => $item->id]);
    }

    // --- Запрос счёта (RES-13) ---

    public function test_request_bill_broadcasts_event(): void
    {
        Event::fake([OrderBillingRequested::class]);

        $order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id'      => $this->table->id,
            'user_id'       => $this->guest->id,
        ]);

        $this->actingAs($this->guest)
            ->patchJson("/api/orders/{$order->id}/request-bill")
            ->assertOk();

        Event::assertDispatched(OrderBillingRequested::class);
    }
}
