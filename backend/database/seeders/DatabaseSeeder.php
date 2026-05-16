<?php

namespace Database\Seeders;

use App\Models\MenuCategory;
use App\Models\MenuItem;
use App\Models\Restaurant;
use App\Models\Table;
use App\Models\User;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;

class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        $central = Restaurant::create([
            'name' => 'RestAll Central',
            'address' => 'Warsaw, ul. Marszałkowska 1',
        ]);

        $bistro = Restaurant::create([
            'name' => 'RestAll Bistro',
            'address' => 'Warsaw, ul. Nowy Świat 42',
        ]);

        $this->seedStaff($central->id, 'central');
        $this->seedStaff($bistro->id, 'bistro');

        User::create([
            'name' => 'Test Customer',
            'email' => 'customer@restall.com',
            'password' => Hash::make('password'),
            'role' => 'guest',
            'restaurant_id' => null,
        ]);

        $this->seedMenu($central->id);
        $this->seedMenu($bistro->id);

        $this->seedTables($central->id);
        $this->seedTables($bistro->id);
    }

    private function seedStaff(int $restaurantId, string $slug): void
    {
        User::create([
            'restaurant_id' => $restaurantId,
            'name' => 'Admin ' . ucfirst($slug),
            'email' => "admin.{$slug}@restall.com",
            'password' => Hash::make('password'),
            'role' => 'admin',
        ]);

        User::create([
            'restaurant_id' => $restaurantId,
            'name' => 'Cashier ' . ucfirst($slug),
            'email' => "cashier.{$slug}@restall.com",
            'password' => Hash::make('password'),
            'role' => 'cashier',
        ]);

        User::create([
            'restaurant_id' => $restaurantId,
            'name' => 'Chef ' . ucfirst($slug),
            'email' => "chef.{$slug}@restall.com",
            'password' => Hash::make('password'),
            'role' => 'chef',
        ]);

        User::create([
            'restaurant_id' => $restaurantId,
            'name' => 'Waiter ' . ucfirst($slug),
            'email' => "waiter.{$slug}@restall.com",
            'password' => Hash::make('password'),
            'role' => 'waiter',
        ]);
    }

    private function seedTables(int $restaurantId): void
    {
        $configs = [
            ['number' => 'T-1', 'capacity' => 2],
            ['number' => 'T-2', 'capacity' => 2],
            ['number' => 'T-3', 'capacity' => 4],
            ['number' => 'T-4', 'capacity' => 4],
            ['number' => 'T-5', 'capacity' => 4],
            ['number' => 'T-6', 'capacity' => 6],
            ['number' => 'T-7', 'capacity' => 6],
            ['number' => 'T-8', 'capacity' => 8],
        ];

        foreach ($configs as $config) {
            Table::create(array_merge($config, [
                'restaurant_id' => $restaurantId,
                'status' => 'free',
            ]));
        }
    }

    private function seedMenu(int $restaurantId): void
    {
        $img = fn (string $id) => "https://images.unsplash.com/photo-{$id}?auto=format&fit=crop&w=600&q=80";

        $categories = [
            [
                'name' => 'Starters',
                'sort_order' => 1,
                'items' => [
                    [
                        'name' => 'Bruschetta',
                        'description' => 'Grilled bread with fresh tomatoes, garlic and basil',
                        'price' => 18.00,
                        'photo_url' => $img('1572695157366-5e585ab2b69f'),
                    ],
                    [
                        'name' => 'Caesar Salad',
                        'description' => 'Romaine lettuce, parmesan, croutons, caesar dressing',
                        'price' => 24.00,
                        'photo_url' => $img('1546069901-ba9599a7e63c'),
                    ],
                    [
                        'name' => 'French Onion Soup',
                        'description' => 'Classic French style with gruyère crouton',
                        'price' => 22.00,
                        'photo_url' => $img('1547592166-23ac45744acd'),
                    ],
                    [
                        'name' => 'Chicken Wings',
                        'description' => '8 pieces with BBQ or buffalo sauce',
                        'price' => 32.00,
                        'photo_url' => $img('1527477396000-e27163b481c2'),
                    ],
                    [
                        'name' => 'Caprese',
                        'description' => 'Fresh mozzarella, tomatoes, basil, extra virgin olive oil',
                        'price' => 28.00,
                        'photo_url' => $img('1591814468924-caf88d1232e1'),
                    ],
                ],
            ],
            [
                'name' => 'Main Dishes',
                'sort_order' => 2,
                'items' => [
                    [
                        'name' => 'Classic Burger',
                        'description' => '200g beef patty, cheddar, tomato, lettuce, brioche bun',
                        'price' => 38.00,
                        'photo_url' => $img('1568901346375-23c9450c58cd'),
                    ],
                    [
                        'name' => 'Ribeye Steak',
                        'description' => '300g dry-aged ribeye, fries, mushroom sauce',
                        'price' => 89.00,
                        'photo_url' => $img('1546833999-b9f581a1996d'),
                    ],
                    [
                        'name' => 'Grilled Salmon',
                        'description' => 'Atlantic salmon with lemon butter sauce and asparagus',
                        'price' => 62.00,
                        'photo_url' => $img('1519708227418-c8fd9a32b7a2'),
                    ],
                    [
                        'name' => 'Pasta Carbonara',
                        'description' => 'Spaghetti, guanciale, egg yolk, pecorino, black pepper',
                        'price' => 36.00,
                        'photo_url' => $img('1612874742237-6526221588e3'),
                    ],
                    [
                        'name' => 'Margherita Pizza',
                        'description' => '32cm, San Marzano tomato sauce, buffalo mozzarella, basil',
                        'price' => 34.00,
                        'photo_url' => $img('1513104890138-7c749659a591'),
                    ],
                    [
                        'name' => 'Chicken Tikka Masala',
                        'description' => 'Tender chicken in spiced tomato cream sauce with basmati rice',
                        'price' => 44.00,
                        'photo_url' => $img('1565557623262-b51206a2c552'),
                    ],
                    [
                        'name' => 'Vegetable Risotto',
                        'description' => 'Arborio rice, seasonal vegetables, parmesan, white wine',
                        'price' => 42.00,
                        'photo_url' => $img('1476124369491-e7dfd55261db'),
                    ],
                ],
            ],
            [
                'name' => 'Desserts',
                'sort_order' => 3,
                'items' => [
                    [
                        'name' => 'Tiramisu',
                        'description' => 'Classic Italian dessert with mascarpone and espresso',
                        'price' => 22.00,
                        'photo_url' => $img('1571877227200-a0d98ea607e9'),
                    ],
                    [
                        'name' => 'Crème Brûlée',
                        'description' => 'Vanilla custard with caramelized sugar crust',
                        'price' => 24.00,
                        'photo_url' => $img('1470124182917-cc6e71b22ecc'),
                    ],
                    [
                        'name' => 'Chocolate Lava Cake',
                        'description' => 'Warm dark chocolate cake with molten center, vanilla ice cream',
                        'price' => 26.00,
                        'photo_url' => $img('1578985545062-69928b1d9587'),
                    ],
                    [
                        'name' => 'Cheesecake',
                        'description' => 'New York style with strawberry compote',
                        'price' => 22.00,
                        'photo_url' => $img('1567171466572-1263ad028ae4'),
                    ],
                ],
            ],
            [
                'name' => 'Drinks',
                'sort_order' => 4,
                'items' => [
                    [
                        'name' => 'Still Water',
                        'description' => '500ml bottle',
                        'price' => 8.00,
                        'photo_url' => null,
                    ],
                    [
                        'name' => 'Sparkling Water',
                        'description' => '500ml bottle',
                        'price' => 9.00,
                        'photo_url' => null,
                    ],
                    [
                        'name' => 'Fresh Orange Juice',
                        'description' => 'Freshly squeezed, 300ml',
                        'price' => 16.00,
                        'photo_url' => $img('1621506289937-a8e4df240d0b'),
                    ],
                    [
                        'name' => 'Espresso',
                        'description' => 'Double shot, Italian roast',
                        'price' => 10.00,
                        'photo_url' => $img('1514432324607-a09d9b932adc'),
                    ],
                    [
                        'name' => 'Craft Beer',
                        'description' => '0.5L, rotating house selection',
                        'price' => 18.00,
                        'photo_url' => $img('1608270586695-97b1dc37b33a'),
                    ],
                    [
                        'name' => 'House Wine',
                        'description' => 'Red or white, 150ml glass',
                        'price' => 22.00,
                        'photo_url' => $img('1510812431401-41d2bd2722f3'),
                    ],
                ],
            ],
        ];

        foreach ($categories as $categoryData) {
            $items = $categoryData['items'];
            unset($categoryData['items']);

            $category = MenuCategory::create(array_merge($categoryData, [
                'restaurant_id' => $restaurantId,
            ]));

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
