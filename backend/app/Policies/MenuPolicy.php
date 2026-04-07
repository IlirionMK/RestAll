<?php

namespace App\Models;

namespace App\Policies;

use App\Models\User;

class MenuPolicy
{
    public function viewAny(?User $user): bool
    {
        return true;
    }

    public function create(User $user): bool
    {
        return $user->role === 'admin';
    }

    public function update(User $user): bool
    {
        return $user->role === 'admin';
    }

    public function toggleAvailability(User $user): bool
    {
        return in_array($user->role, ['admin', 'waiter']);
    }
}
