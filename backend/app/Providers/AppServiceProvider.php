<?php

namespace App\Providers;

use App\Models\OrderItem;
use App\Policies\KitchenPolicy;
use Illuminate\Support\Facades\Gate;
use Illuminate\Support\ServiceProvider;

class AppServiceProvider extends ServiceProvider
{
    public function register(): void {}

    public function boot(): void
    {
        Gate::policy(OrderItem::class, KitchenPolicy::class);
    }
}
