<?php

declare(strict_types=1);

namespace App\Policies;

use App\Models\User;
use App\Models\MenuCategory;

class MenuCategoryPolicy
{
    public function viewAny(?User $user): bool
    {
        return true;
    }

    public function view(?User $user, MenuCategory $menuCategory): bool
    {
        return true;
    }

    public function create(User $user): bool
    {
        return $user->role === 'admin';
    }

    public function update(User $user, MenuCategory $menuCategory): bool
    {
        return $user->role === 'admin';
    }

    public function delete(User $user, MenuCategory $menuCategory): bool
    {
        return $user->role === 'admin';
    }
}
