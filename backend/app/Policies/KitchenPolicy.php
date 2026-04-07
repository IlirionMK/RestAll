<?php

namespace App\Policies;

use App\Models\User;

class KitchenPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, ['chef', 'admin']);
    }

    public function updateStatus(User $user): bool
    {
        return in_array($user->role, ['chef', 'admin']);
    }
}
