<?php

namespace App\Policies;

use App\Models\User;

class TablePolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, ['admin', 'waiter', 'chef']);
    }

    public function updateStatus(User $user): bool
    {
        return in_array($user->role, ['admin', 'waiter']);
    }
}
