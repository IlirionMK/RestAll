<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Models\Restaurant;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class UserApiTest extends TestCase
{
    use RefreshDatabase;

    private Restaurant $restaurant;

    private User $admin;

    private User $waiter;

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
    }

    // --- Профиль ---

    public function test_user_can_get_own_profile(): void
    {
        $this->actingAs($this->waiter)
            ->getJson('/api/users/me')
            ->assertOk()
            ->assertJsonFragment(['id' => $this->waiter->id, 'email' => $this->waiter->email]);
    }

    public function test_unauthenticated_cannot_get_profile(): void
    {
        $this->getJson('/api/users/me')->assertUnauthorized();
    }

    public function test_user_can_update_own_name(): void
    {
        $this->actingAs($this->waiter)
            ->putJson('/api/users/me', ['name' => 'New Name'])
            ->assertOk();

        $this->assertDatabaseHas('users', ['id' => $this->waiter->id, 'name' => 'New Name']);
    }

    // --- Управление персоналом ---

    public function test_admin_can_list_staff(): void
    {
        $this->actingAs($this->admin)
            ->getJson('/api/users')
            ->assertOk();
    }

    public function test_waiter_cannot_list_staff(): void
    {
        $this->actingAs($this->waiter)
            ->getJson('/api/users')
            ->assertForbidden();
    }

    public function test_admin_can_create_staff_member(): void
    {
        $this->actingAs($this->admin)
            ->postJson('/api/users', [
                'name' => 'New Chef',
                'email' => 'newchef@restall.com',
                'password' => 'password123',
                'role' => 'chef',
            ])
            ->assertStatus(201);

        $this->assertDatabaseHas('users', ['email' => 'newchef@restall.com', 'role' => 'chef']);
    }

    public function test_waiter_cannot_create_staff_member(): void
    {
        $this->actingAs($this->waiter)
            ->postJson('/api/users', [
                'name' => 'New Chef',
                'email' => 'newchef@restall.com',
                'password' => 'password123',
                'role' => 'chef',
            ])
            ->assertForbidden();
    }

    public function test_admin_can_update_user_role(): void
    {
        $this->actingAs($this->admin)
            ->patchJson("/api/users/{$this->waiter->id}/role", ['role' => 'chef'])
            ->assertOk();

        $this->assertDatabaseHas('users', ['id' => $this->waiter->id, 'role' => 'chef']);
    }

    public function test_waiter_cannot_update_role(): void
    {
        $this->actingAs($this->waiter)
            ->patchJson("/api/users/{$this->admin->id}/role", ['role' => 'guest'])
            ->assertForbidden();
    }

    public function test_admin_can_delete_staff_member(): void
    {
        $this->actingAs($this->admin)
            ->deleteJson("/api/users/{$this->waiter->id}")
            ->assertNoContent();

        $this->assertDatabaseMissing('users', ['id' => $this->waiter->id, 'deleted_at' => null]);
    }

    public function test_waiter_cannot_delete_user(): void
    {
        $chef = User::factory()->create([
            'role' => 'chef',
            'restaurant_id' => $this->restaurant->id,
        ]);

        $this->actingAs($this->waiter)
            ->deleteJson("/api/users/{$chef->id}")
            ->assertForbidden();
    }
}
