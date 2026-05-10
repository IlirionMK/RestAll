<?php

declare(strict_types=1);

namespace App\Policies;

use App\Enums\UserRole;
use App\Models\User;

class TablePolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [UserRole::ADMIN, UserRole::WAITER, UserRole::CHEF]);
    }

    public function updateStatus(User $user): bool
    {
        return in_array($user->role, [UserRole::ADMIN, UserRole::WAITER]);
    }
}
