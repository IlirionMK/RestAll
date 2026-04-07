<?php

namespace Database\Seeders;

use App\Models\Restaurant;
use App\Models\User;
use App\Models\MenuCategory;
use App\Models\MenuItem;
use App\Models\Table;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;

class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        $restaurant = Restaurant::create([
            'name' => 'RestAll Central',
            'address' => 'Warsaw, Main St 1'
        ]);

        User::create([
            'restaurant_id' => $restaurant->id,
            'name' => 'Admin User',
            'email' => 'admin@restall.com',
            'password' => Hash::make('password'),
            'role' => 'admin'
        ]);

        User::create([
            'restaurant_id' => $restaurant->id,
            'name' => 'John Waiter',
            'email' => 'waiter@restall.com',
            'password' => Hash::make('password'),
            'role' => 'waiter'
        ]);

        User::create([
            'restaurant_id' => $restaurant->id,
            'name' => 'Chef Mario',
            'email' => 'chef@restall.com',
            'password' => Hash::make('password'),
            'role' => 'chef'
        ]);

        $category = MenuCategory::create([
            'restaurant_id' => $restaurant->id,
            'name' => 'Main Dishes',
            'sort_order' => 1
        ]);

        MenuItem::create([
            'restaurant_id' => $restaurant->id,
            'menu_category_id' => $category->id,
            'name' => 'Classic Burger',
            'description' => 'Beef, cheese, tomato',
            'price' => 15.50,
            'is_available' => true
        ]);

        for ($i = 1; $i <= 5; $i++) {
            Table::create([
                'restaurant_id' => $restaurant->id,
                'number' => 'T-' . $i,
                'capacity' => 4,
                'status' => 'free'
            ]);
        }
    }
}
