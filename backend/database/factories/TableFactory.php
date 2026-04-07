<?php

namespace Database\Factories;

use App\Models\Restaurant;
use Illuminate\Database\Eloquent\Factories\Factory;

class TableFactory extends Factory
{
    public function definition(): array
    {
        return [
            'restaurant_id' => Restaurant::factory(),
            'number' => $this->faker->unique()->numberBetween(1, 50),
            'capacity' => $this->faker->numberBetween(2, 10),
            'status' => 'free',
        ];
    }
}
