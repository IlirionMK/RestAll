<?php

declare(strict_types=1);

namespace Database\Factories;

use App\Enums\OrderStatus;
use App\Models\Restaurant;
use App\Models\Table;
use App\Models\User;
use Illuminate\Database\Eloquent\Factories\Factory;

class OrderFactory extends Factory
{
    public function definition(): array
    {
        return [
            'restaurant_id' => Restaurant::factory(),
            'table_id' => Table::factory(),
            'user_id' => User::factory(),
            'status' => OrderStatus::PENDING,
            'total_amount' => 0,
            'is_takeaway' => false,
        ];
    }
}
