<?php

namespace App\Providers;

use App\Models\MenuCategory;
use App\Models\MenuItem;
use App\Models\OrderItem;
use App\Policies\KitchenPolicy;
use App\Policies\MenuPolicy;
use Illuminate\Support\Facades\Gate;
use Illuminate\Support\ServiceProvider;

class AppServiceProvider extends ServiceProvider
{
    public function register(): void {}

    public function boot(): void
    {
        Gate::policy(OrderItem::class, KitchenPolicy::class);
        Gate::policy(MenuItem::class, MenuPolicy::class);
        Gate::policy(MenuCategory::class, MenuPolicy::class);
    }
}
