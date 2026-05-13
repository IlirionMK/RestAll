<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Models\MenuCategory;
use App\Models\MenuItem;
use App\Models\Restaurant;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class MenuApiTest extends TestCase
{
    use RefreshDatabase;

    private Restaurant $restaurant;

    private User $admin;

    private User $waiter;

    private User $chef;

    private MenuCategory $category;

    protected function setUp(): void
    {
        parent::setUp();

        $this->restaurant = Restaurant::factory()->create();

        $this->admin = User::factory()->create([
            'role' => 'admin',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->waiter = User::factory()->create([
            'role' => 'waiter',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->chef = User::factory()->create([
            'role' => 'chef',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->category = MenuCategory::factory()->create([
            'restaurant_id' => $this->restaurant->id,
        ]);
    }

    public function test_unauthenticated_can_view_menu_categories_by_restaurant(): void
    {
        MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
            'is_available' => true,
        ]);

        $this->getJson('/api/menu/categories?restaurant_id='.$this->restaurant->id)
            ->assertOk()
            ->assertJsonCount(1);
    }

    public function test_authenticated_user_can_view_menu_categories(): void
    {
        MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
            'is_available' => true,
        ]);

        $this->actingAs($this->admin)
            ->getJson('/api/menu/categories')
            ->assertOk()
            ->assertJsonCount(1);
    }

    public function test_categories_exclude_unavailable_items(): void
    {
        MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
            'is_available' => true,
        ]);

        MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
            'is_available' => false,
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson('/api/menu/categories')
            ->assertOk();

        $this->assertCount(1, $response->json('0.items'));
    }

    public function test_admin_can_create_menu_item(): void
    {
        $this->actingAs($this->admin)
            ->postJson('/api/menu/items', [
                'menu_category_id' => $this->category->id,
                'name' => 'New Dish',
                'price' => 25.00,
                'is_available' => true,
            ])
            ->assertStatus(201);

        $this->assertDatabaseHas('menu_items', ['name' => 'New Dish']);
    }

    public function test_waiter_cannot_create_menu_item(): void
    {
        $this->actingAs($this->waiter)
            ->postJson('/api/menu/items', [
                'menu_category_id' => $this->category->id,
                'name' => 'New Dish',
                'price' => 25.00,
            ])
            ->assertForbidden();
    }

    public function test_chef_cannot_create_menu_item(): void
    {
        $this->actingAs($this->chef)
            ->postJson('/api/menu/items', [
                'menu_category_id' => $this->category->id,
                'name' => 'New Dish',
                'price' => 25.00,
            ])
            ->assertForbidden();
    }

    public function test_admin_can_update_menu_item(): void
    {
        $item = MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
        ]);

        $this->actingAs($this->admin)
            ->putJson("/api/menu/items/{$item->id}", ['name' => 'Updated Name', 'price' => 30.00])
            ->assertOk();

        $this->assertDatabaseHas('menu_items', ['id' => $item->id, 'name' => 'Updated Name']);
    }

    public function test_waiter_cannot_update_menu_item(): void
    {
        $item = MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
        ]);

        $this->actingAs($this->waiter)
            ->putJson("/api/menu/items/{$item->id}", ['name' => 'Hacked', 'price' => 1.00])
            ->assertForbidden();
    }

    public function test_admin_can_toggle_item_availability(): void
    {
        $item = MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
            'is_available' => true,
        ]);

        $this->actingAs($this->admin)
            ->patchJson("/api/menu/items/{$item->id}/availability")
            ->assertOk();

        $this->assertDatabaseHas('menu_items', ['id' => $item->id, 'is_available' => false]);
    }

    public function test_waiter_can_toggle_item_availability(): void
    {
        $item = MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
            'is_available' => true,
        ]);

        $this->actingAs($this->waiter)
            ->patchJson("/api/menu/items/{$item->id}/availability")
            ->assertOk();
    }

    public function test_chef_cannot_toggle_item_availability(): void
    {
        $item = MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
        ]);

        $this->actingAs($this->chef)
            ->patchJson("/api/menu/items/{$item->id}/availability")
            ->assertForbidden();
    }

    public function test_admin_can_delete_menu_item(): void
    {
        $item = MenuItem::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'menu_category_id' => $this->category->id,
        ]);

        $this->actingAs($this->admin)
            ->deleteJson("/api/menu/items/{$item->id}")
            ->assertNoContent();

        $this->assertDatabaseMissing('menu_items', ['id' => $item->id]);
    }
}
