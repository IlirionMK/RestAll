<?php

declare(strict_types=1);

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\MenuCategory;
use App\Models\User;

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
        return $user->role === UserRole::ADMIN;
    }

    public function update(User $user, MenuCategory $menuCategory): bool
    {
        return $user->role === UserRole::ADMIN;
    }

    public function delete(User $user, MenuCategory $menuCategory): bool
    {
        return $user->role === UserRole::ADMIN;
    }
}
