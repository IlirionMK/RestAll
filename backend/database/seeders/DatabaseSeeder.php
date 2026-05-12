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

        $this->seedMenu($restaurant->id);

        for ($i = 1; $i <= 8; $i++) {
            Table::create([
                'restaurant_id' => $restaurant->id,
                'number' => 'T-' . $i,
                'capacity' => $i <= 4 ? 2 : 4,
                'status' => 'free'
            ]);
        }
    }

    private function seedMenu(int $restaurantId): void
    {
        $categories = [
            [
                'name' => 'Starters',
                'sort_order' => 1,
                'items' => [
                    ['name' => 'Bruschetta', 'description' => 'Grilled bread with tomatoes, garlic and basil', 'price' => 18.00],
                    ['name' => 'Caesar Salad', 'description' => 'Romaine lettuce, parmesan, croutons, caesar dressing', 'price' => 24.00],
                    ['name' => 'Onion Soup', 'description' => 'French style with gruyere crouton', 'price' => 22.00],
                    ['name' => 'Chicken Wings', 'description' => '8 pieces with BBQ or buffalo sauce', 'price' => 32.00],
                ],
            ],
            [
                'name' => 'Main Dishes',
                'sort_order' => 2,
                'items' => [
                    ['name' => 'Classic Burger', 'description' => '200g beef patty, cheddar, tomato, lettuce', 'price' => 38.00],
                    ['name' => 'Ribeye Steak', 'description' => '300g aged ribeye, served with fries and sauce', 'price' => 89.00],
                    ['name' => 'Grilled Salmon', 'description' => 'With lemon butter sauce and asparagus', 'price' => 62.00],
                    ['name' => 'Pasta Carbonara', 'description' => 'Spaghetti, pancetta, egg, parmesan, black pepper', 'price' => 36.00],
                    ['name' => 'Margherita Pizza', 'description' => '32cm, tomato sauce, mozzarella, fresh basil', 'price' => 34.00],
                    ['name' => 'Chicken Tikka Masala', 'description' => 'Tender chicken in spiced tomato cream sauce with rice', 'price' => 44.00],
                ],
            ],
            [
                'name' => 'Desserts',
                'sort_order' => 3,
                'items' => [
                    ['name' => 'Tiramisu', 'description' => 'Classic Italian dessert with mascarpone', 'price' => 22.00],
                    ['name' => 'Crème Brûlée', 'description' => 'Vanilla custard with caramelized sugar crust', 'price' => 24.00],
                    ['name' => 'Chocolate Lava Cake', 'description' => 'Warm chocolate cake with liquid center, vanilla ice cream', 'price' => 26.00],
                ],
            ],
            [
                'name' => 'Drinks',
                'sort_order' => 4,
                'items' => [
                    ['name' => 'Still Water', 'description' => '500ml bottle', 'price' => 8.00],
                    ['name' => 'Sparkling Water', 'description' => '500ml bottle', 'price' => 9.00],
                    ['name' => 'Fresh Orange Juice', 'description' => 'Freshly squeezed, 300ml', 'price' => 16.00],
                    ['name' => 'Espresso', 'description' => 'Double shot', 'price' => 10.00],
                    ['name' => 'Craft Beer', 'description' => '0.5L, house selection', 'price' => 18.00],
                    ['name' => 'House Wine', 'description' => 'Red or white, 150ml', 'price' => 22.00],
                ],
            ],
        ];

        foreach ($categories as $categoryData) {
            $items = $categoryData['items'];
            unset($categoryData['items']);

            $category = MenuCategory::create(array_merge($categoryData, ['restaurant_id' => $restaurantId]));

            foreach ($items as $itemData) {
                MenuItem::create(array_merge($itemData, [
                    'restaurant_id' => $restaurantId,
                    'menu_category_id' => $category->id,
                    'is_available' => true,
                ]));
            }
        }
    }
}
