<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Enums\OrderItemStatus;
use App\Events\KitchenOrderItemsAdded;
use App\Events\KitchenTicketStatusUpdated;
use App\Events\WaiterItemReady;
use App\Models\MenuItem;
use App\Models\Order;
use App\Models\OrderItem;
use App\Models\Restaurant;
use App\Models\Table;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Event;
use Tests\TestCase;

class KitchenApiTest extends TestCase
{
    use RefreshDatabase;

    private Restaurant $restaurant;

    private User $chef;

    private Order $order;

    protected function setUp(): void
    {
        parent::setUp();

        $this->restaurant = Restaurant::factory()->create();

        $this->chef = User::factory()->create([
            'role' => 'chef',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $waiter = User::factory()->create([
            'role' => 'waiter',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $table = Table::factory()->create(['restaurant_id' => $this->restaurant->id]);

        $this->order = Order::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $table->id,
            'user_id' => $waiter->id,
        ]);
    }

    private function makeItem(array $attrs = []): OrderItem
    {
        $menuItem = MenuItem::factory()->create(['restaurant_id' => $this->restaurant->id]);

        return OrderItem::factory()->create(array_merge([
            'order_id' => $this->order->id,
            'menu_item_id' => $menuItem->id,
        ], $attrs));
    }

    // --- RES-14: список тикетов ---

    public function test_chef_sees_pending_and_preparing_tickets(): void
    {
        $pending = $this->makeItem();
        $preparing = $this->makeItem(['status' => OrderItemStatus::PREPARING]);
        $ready = $this->makeItem(['status' => OrderItemStatus::READY]);

        $response = $this->actingAs($this->chef)
            ->getJson('/api/kitchen/tickets');

        $response->assertOk()
            ->assertJsonCount(2, 'data')
            ->assertJsonFragment(['id' => $pending->id, 'status' => 'pending'])
            ->assertJsonFragment(['id' => $preparing->id, 'status' => 'preparing'])
            ->assertJsonMissing(['id' => $ready->id]);
    }

    public function test_ticket_response_includes_table_number(): void
    {
        $this->makeItem();

        $response = $this->actingAs($this->chef)->getJson('/api/kitchen/tickets');

        $response->assertOk()
            ->assertJsonPath('data.0.table_number', $this->order->table->number);
    }

    public function test_waiter_cannot_access_kitchen_tickets(): void
    {
        $waiter = User::factory()->create([
            'role' => 'waiter',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->actingAs($waiter)
            ->getJson('/api/kitchen/tickets')
            ->assertForbidden();
    }

    public function test_unauthenticated_cannot_access_kitchen_tickets(): void
    {
        $this->getJson('/api/kitchen/tickets')->assertUnauthorized();
    }

    // --- RES-15: смена статуса ---

    public function test_chef_can_advance_ticket_status(): void
    {
        Event::fake([KitchenTicketStatusUpdated::class]);

        $item = $this->makeItem();

        $this->actingAs($this->chef)
            ->patchJson("/api/kitchen/tickets/{$item->id}/status", ['status' => 'preparing'])
            ->assertOk()
            ->assertJsonFragment(['status' => 'preparing']);

        $this->assertDatabaseHas('order_items', [
            'id' => $item->id,
            'status' => 'preparing',
        ]);

        Event::assertDispatched(KitchenTicketStatusUpdated::class);
    }

    public function test_full_status_progression(): void
    {
        Event::fake([KitchenTicketStatusUpdated::class]);

        $item = $this->makeItem();

        $this->actingAs($this->chef)
            ->patchJson("/api/kitchen/tickets/{$item->id}/status", ['status' => 'preparing'])
            ->assertOk();

        $this->actingAs($this->chef)
            ->patchJson("/api/kitchen/tickets/{$item->id}/status", ['status' => 'ready'])
            ->assertOk()
            ->assertJsonFragment(['status' => 'ready']);

        Event::assertDispatchedTimes(KitchenTicketStatusUpdated::class, 2);
    }

    public function test_backward_transition_returns_422(): void
    {
        $item = $this->makeItem(['status' => OrderItemStatus::PREPARING]);

        $this->actingAs($this->chef)
            ->patchJson("/api/kitchen/tickets/{$item->id}/status", ['status' => 'pending'])
            ->assertUnprocessable();
    }

    public function test_skip_transition_returns_422(): void
    {
        $item = $this->makeItem();

        $this->actingAs($this->chef)
            ->patchJson("/api/kitchen/tickets/{$item->id}/status", ['status' => 'ready'])
            ->assertUnprocessable();
    }

    public function test_waiter_cannot_update_ticket_status(): void
    {
        $waiter = User::factory()->create([
            'role' => 'waiter',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $item = $this->makeItem();

        $this->actingAs($waiter)
            ->patchJson("/api/kitchen/tickets/{$item->id}/status", ['status' => 'preparing'])
            ->assertForbidden();
    }

    // --- RES-16: push-уведомление официанту ---

    public function test_waiter_notified_when_item_becomes_ready(): void
    {
        Event::fake([KitchenTicketStatusUpdated::class, WaiterItemReady::class]);

        $item = $this->makeItem(['status' => OrderItemStatus::PREPARING]);

        $this->actingAs($this->chef)
            ->patchJson("/api/kitchen/tickets/{$item->id}/status", ['status' => 'ready'])
            ->assertOk();

        Event::assertDispatched(WaiterItemReady::class, function (WaiterItemReady $event) use ($item) {
            return $event->orderItem->id === $item->id;
        });
    }

    public function test_waiter_not_notified_on_intermediate_status(): void
    {
        Event::fake([KitchenTicketStatusUpdated::class, WaiterItemReady::class]);

        $item = $this->makeItem();

        $this->actingAs($this->chef)
            ->patchJson("/api/kitchen/tickets/{$item->id}/status", ['status' => 'preparing'])
            ->assertOk();

        Event::assertNotDispatched(WaiterItemReady::class);
    }

    // --- RES-14: broadcast при добавлении блюд ---

    public function test_adding_order_items_broadcasts_to_kitchen(): void
    {
        Event::fake([KitchenOrderItemsAdded::class]);

        $waiter = User::factory()->create([
            'role' => 'waiter',
            'restaurant_id' => $this->restaurant->id,
        ]);
        $menuItem = MenuItem::factory()->create(['restaurant_id' => $this->restaurant->id]);

        $this->actingAs($waiter)->postJson("/api/orders/{$this->order->id}/items", [
            'items' => [['menu_item_id' => $menuItem->id, 'quantity' => 1]],
        ])->assertStatus(201);

        Event::assertDispatched(KitchenOrderItemsAdded::class);
    }
}
