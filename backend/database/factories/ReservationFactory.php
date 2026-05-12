<?php

declare(strict_types=1);

namespace Database\Factories;

use App\Enums\ReservationStatus;
use App\Models\Restaurant;
use App\Models\Table;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;

class ReservationFactory extends Factory
{
    public function definition(): array
    {
        return [
            'restaurant_id' => Restaurant::factory(),
            'table_id' => Table::factory(),
            'user_id' => User::factory(),
            'reservation_time' => now()->addDay()->format('Y-m-d H:i:s'),
            'guests_count' => fake()->numberBetween(1, 8),
            'status' => ReservationStatus::PENDING,
        ];
    }

    public function confirmed(): static
    {
        return $this->state(['status' => ReservationStatus::CONFIRMED]);
    }

    public function cancelled(): static
    {
        return $this->state(['status' => ReservationStatus::CANCELLED]);
    }
}
