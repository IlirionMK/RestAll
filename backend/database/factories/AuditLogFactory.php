<?php

declare(strict_types=1);

namespace Database\Factories;

use App\Models\Restaurant;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;

class AuditLogFactory extends Factory
{
    public function definition(): array
    {
        return [
            'restaurant_id' => Restaurant::factory(),
            'user_id' => User::factory(),
            'action' => fake()->randomElement(['order.created', 'order.paid', 'reservation.created', 'user.login']),
            'model_type' => 'App\\Models\\Order',
            'model_id' => fake()->randomNumber(),
            'payload' => [],
            'ip_address' => fake()->ipv4(),
        ];
    }
}
