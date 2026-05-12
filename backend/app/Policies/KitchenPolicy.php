<?php

declare(strict_types=1);

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\User;

class KitchenPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [UserRole::CHEF, UserRole::ADMIN]);
    }

    public function updateStatus(User $user): bool
    {
        return in_array($user->role, [UserRole::CHEF, UserRole::ADMIN]);
    }

    public function delete(User $user): bool
    {
        return in_array($user->role, [UserRole::WAITER, UserRole::ADMIN]);
    }
}
