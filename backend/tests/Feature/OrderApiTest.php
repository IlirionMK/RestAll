<?php

namespace Tests\Feature;

use App\Models\User;
use App\Models\Table;
use App\Models\Restaurant;
use App\Models\MenuItem;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class OrderApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_complete_order_lifecycle(): void
    {
        $restaurant = Restaurant::factory()->create();
        $waiter = User::factory()->create([
            'role' => 'waiter',
            'restaurant_id' => $restaurant->id
        ]);
        $table = Table::factory()->create([
            'restaurant_id' => $restaurant->id,
            'status' => 'free'
        ]);
        $menuItem = MenuItem::factory()->create([
            'restaurant_id' => $restaurant->id,
            'price' => 150.00
        ]);

        $orderResponse = $this->actingAs($waiter)->postJson('/api/orders', [
            'table_id' => $table->id
        ]);

        $orderResponse->assertStatus(201);
        $orderId = $orderResponse->json('id');

        $this->actingAs($waiter)->postJson("/api/orders/{$orderId}/items", [
            'items' => [
                ['menu_item_id' => $menuItem->id, 'quantity' => 2]
            ]
        ])->assertStatus(201);

        $this->assertDatabaseHas('orders', [
            'id' => $orderId,
            'total_amount' => 300.00
        ]);

        $this->actingAs($waiter)->patchJson("/api/orders/{$orderId}/pay")
            ->assertStatus(200);

        $this->assertDatabaseHas('orders', [
            'id' => $orderId,
            'status' => 'paid'
        ]);

        $this->assertDatabaseHas('tables', [
            'id' => $table->id,
            'status' => 'free'
        ]);

        $this->assertDatabaseHas('audit_logs', [
            'user_id' => $waiter->id,
            'action' => 'order.paid'
        ]);
    }
}
