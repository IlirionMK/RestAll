<?php

declare(strict_types=1);

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\User;

class MenuPolicy
{
    public function viewAny(?User $user): bool
    {
        return true;
    }

    public function create(User $user): bool
    {
        return $user->role === UserRole::ADMIN;
    }

    public function update(User $user): bool
    {
        return $user->role === UserRole::ADMIN;
    }

    public function toggleAvailability(User $user): bool
    {
        return in_array($user->role, [UserRole::ADMIN, UserRole::WAITER]);
    }
}
