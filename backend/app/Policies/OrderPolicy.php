<?php

namespace App\Policies;

use App\Models\User;
use App\Models\Order;

class OrderPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, ['admin', 'waiter']);
    }

    public function create(User $user): bool
    {
        return $user->role === 'waiter';
    }

    public function addItems(User $user, Order $order): bool
    {
        return $user->role === 'waiter' && $order->status === 'pending';
    }

    public function pay(User $user, Order $order): bool
    {
        return in_array($user->role, ['admin', 'waiter']) && $order->status === 'pending';
    }

    public function close(User $user): bool
    {
        return in_array($user->role, ['admin', 'waiter']);
    }
}
