<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Models\Restaurant;
use App\Models\Table;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class TableApiTest extends TestCase
{
    use RefreshDatabase;

    private Restaurant $restaurant;
    private User $admin;
    private User $waiter;
    private User $chef;

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

        $this->chef = User::factory()->create([
            'role'          => 'chef',
            'restaurant_id' => $this->restaurant->id,
        ]);
    }

    public function test_authenticated_user_can_list_tables(): void
    {
        Table::factory()->count(3)->create(['restaurant_id' => $this->restaurant->id]);

        $this->actingAs($this->waiter)
            ->getJson("/api/tables?restaurant_id={$this->restaurant->id}")
            ->assertOk()
            ->assertJsonCount(3);
    }

    public function test_unauthenticated_cannot_list_tables(): void
    {
        $this->getJson("/api/tables?restaurant_id={$this->restaurant->id}")
            ->assertUnauthorized();
    }

    public function test_tables_filtered_by_restaurant_id(): void
    {
        $other = Restaurant::factory()->create();
        Table::factory()->count(2)->create(['restaurant_id' => $this->restaurant->id]);
        Table::factory()->count(3)->create(['restaurant_id' => $other->id]);

        $this->actingAs($this->admin)
            ->getJson("/api/tables?restaurant_id={$this->restaurant->id}")
            ->assertOk()
            ->assertJsonCount(2);
    }

    public function test_waiter_can_update_table_status(): void
    {
        $table = Table::factory()->create(['restaurant_id' => $this->restaurant->id, 'status' => 'free']);

        $this->actingAs($this->waiter)
            ->patchJson("/api/tables/{$table->id}/status", ['status' => 'cleaning'])
            ->assertOk();

        $this->assertDatabaseHas('tables', ['id' => $table->id, 'status' => 'cleaning']);
    }

    public function test_admin_can_update_table_status(): void
    {
        $table = Table::factory()->create(['restaurant_id' => $this->restaurant->id]);

        $this->actingAs($this->admin)
            ->patchJson("/api/tables/{$table->id}/status", ['status' => 'occupied'])
            ->assertOk();
    }

    public function test_chef_cannot_update_table_status(): void
    {
        $table = Table::factory()->create(['restaurant_id' => $this->restaurant->id]);

        $this->actingAs($this->chef)
            ->patchJson("/api/tables/{$table->id}/status", ['status' => 'cleaning'])
            ->assertForbidden();
    }

    public function test_update_table_status_requires_valid_status(): void
    {
        $table = Table::factory()->create(['restaurant_id' => $this->restaurant->id]);

        $this->actingAs($this->waiter)
            ->patchJson("/api/tables/{$table->id}/status", ['status' => 'invalid_status'])
            ->assertUnprocessable();
    }
}
