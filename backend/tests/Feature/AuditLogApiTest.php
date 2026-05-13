<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Models\AuditLog;
use App\Models\Restaurant;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Carbon;
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
            ->assertOk()
            ->assertJsonStructure(['data', 'current_page', 'total', 'per_page']);
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

    public function test_filter_by_action(): void
    {
        AuditLog::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'user_id' => $this->waiter->id,
            'action' => 'order.paid',
        ]);

        AuditLog::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'user_id' => $this->waiter->id,
            'action' => 'order.created',
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson('/api/logs?action=order.paid')
            ->assertOk();

        $this->assertCount(1, $response->json('data'));
        $this->assertEquals('order.paid', $response->json('data.0.action'));
    }

    public function test_filter_by_user_id(): void
    {
        AuditLog::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'user_id' => $this->waiter->id,
            'action' => 'order.created',
        ]);

        AuditLog::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'user_id' => $this->admin->id,
            'action' => 'order.created',
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson("/api/logs?user_id={$this->waiter->id}")
            ->assertOk();

        $this->assertCount(1, $response->json('data'));
        $this->assertEquals($this->waiter->id, $response->json('data.0.user_id'));
    }

    public function test_filter_by_date_range(): void
    {
        AuditLog::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'user_id' => $this->waiter->id,
            'action' => 'order.created',
            'created_at' => Carbon::yesterday(),
        ]);

        AuditLog::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'user_id' => $this->waiter->id,
            'action' => 'order.created',
            'created_at' => Carbon::today(),
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson('/api/logs?date_from='.Carbon::today()->toDateString().'&date_to='.Carbon::today()->toDateString())
            ->assertOk();

        $this->assertCount(1, $response->json('data'));
    }

    public function test_per_page_parameter(): void
    {
        AuditLog::factory()->count(5)->create([
            'restaurant_id' => $this->restaurant->id,
            'user_id' => $this->waiter->id,
        ]);

        $response = $this->actingAs($this->admin)
            ->getJson('/api/logs?per_page=2')
            ->assertOk();

        $this->assertCount(2, $response->json('data'));
        $this->assertEquals(5, $response->json('total'));
    }
}
