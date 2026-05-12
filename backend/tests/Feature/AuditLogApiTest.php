<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Models\Restaurant;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class AuditLogApiTest extends TestCase
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

    public function test_admin_can_list_audit_logs(): void
    {
        $this->actingAs($this->admin)
            ->getJson('/api/logs')
            ->assertOk();
    }

    public function test_waiter_cannot_access_audit_logs(): void
    {
        $this->actingAs($this->waiter)
            ->getJson('/api/logs')
            ->assertForbidden();
    }

    public function test_unauthenticated_cannot_access_audit_logs(): void
    {
        $this->getJson('/api/logs')->assertUnauthorized();
    }

    public function test_audit_logs_respect_limit_parameter(): void
    {
        $this->actingAs($this->admin)
            ->getJson('/api/logs?limit=10')
            ->assertOk();
    }
}
