<?php

declare(strict_types=1);

namespace Database\Factories;

use App\Enums\OrderItemStatus;
use App\Models\MenuItem;
use App\Models\Order;
use Illuminate\Database\Eloquent\Factories\Factory;

class OrderItemFactory extends Factory
{
    public function definition(): array
    {
        return [
            'order_id' => Order::factory(),
            'menu_item_id' => MenuItem::factory(),
            'name' => fake()->word(),
            'price' => fake()->randomFloat(2, 10, 100),
            'quantity' => fake()->numberBetween(1, 5),
            'status' => OrderItemStatus::PENDING,
            'comment' => null,
        ];
    }

    public function preparing(): static
    {
        return $this->state(['status' => OrderItemStatus::PREPARING]);
    }

    public function ready(): static
    {
        return $this->state(['status' => OrderItemStatus::READY]);
    }
}
