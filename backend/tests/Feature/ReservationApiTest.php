<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Models\Reservation;
use App\Models\Restaurant;
use App\Models\Table;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class ReservationApiTest extends TestCase
{
    use RefreshDatabase;

    private Restaurant $restaurant;

    private User $admin;

    private User $waiter;

    private User $guest;

    private Table $table;

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

        $this->guest = User::factory()->create(['role' => 'guest']);

        $this->table = Table::factory()->create(['restaurant_id' => $this->restaurant->id]);
    }

    public function test_guest_can_create_reservation(): void
    {
        $this->actingAs($this->guest)
            ->postJson('/api/reservations', [
                'restaurant_id' => $this->restaurant->id,
                'table_id' => $this->table->id,
                'reservation_time' => now()->addDay()->format('Y-m-d H:i:s'),
                'guests_count' => 2,
            ])
            ->assertStatus(201);

        $this->assertDatabaseHas('reservations', [
            'table_id' => $this->table->id,
            'user_id' => $this->guest->id,
        ]);
    }

    public function test_staff_can_see_all_restaurant_reservations(): void
    {
        Reservation::factory()->count(3)->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
        ]);

        $this->actingAs($this->waiter)
            ->getJson('/api/reservations')
            ->assertOk()
            ->assertJsonCount(3);
    }

    public function test_guest_sees_only_own_reservations(): void
    {
        Reservation::factory()->count(2)->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->guest->id,
        ]);

        $other = User::factory()->create(['role' => 'guest']);
        Reservation::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $other->id,
        ]);

        $this->actingAs($this->guest)
            ->getJson('/api/reservations')
            ->assertOk()
            ->assertJsonCount(2);
    }

    public function test_owner_can_cancel_own_reservation(): void
    {
        $reservation = Reservation::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->guest->id,
        ]);

        $this->actingAs($this->guest)
            ->deleteJson("/api/reservations/{$reservation->id}")
            ->assertNoContent();
    }

    public function test_admin_can_cancel_any_reservation(): void
    {
        $reservation = Reservation::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->guest->id,
        ]);

        $this->actingAs($this->admin)
            ->deleteJson("/api/reservations/{$reservation->id}")
            ->assertNoContent();
    }

    public function test_other_guest_cannot_cancel_reservation(): void
    {
        $reservation = Reservation::factory()->create([
            'restaurant_id' => $this->restaurant->id,
            'table_id' => $this->table->id,
            'user_id' => $this->guest->id,
        ]);

        $otherGuest = User::factory()->create(['role' => 'guest']);

        $this->actingAs($otherGuest)
            ->deleteJson("/api/reservations/{$reservation->id}")
            ->assertForbidden();
    }

    public function test_unauthenticated_cannot_list_reservations(): void
    {
        $this->getJson('/api/reservations')->assertUnauthorized();
    }
}
