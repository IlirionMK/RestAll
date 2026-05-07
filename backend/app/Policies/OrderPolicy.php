<?php

declare(strict_types=1);

namespace App\Policies;

use App\Models\User;
use App\Models\Order;

class OrderPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, ['admin', 'waiter', 'chef']);
    }

    public function view(User $user, Order $order): bool
    {
        if (in_array($user->role, ['admin', 'waiter', 'chef'])) {
            return true;
        }

        return $user->id === $order->user_id;
    }

    public function create(User $user): bool
    {
        return in_array($user->role, ['waiter', 'guest']);
    }

    public function addItems(User $user, Order $order): bool
    {
        if ($order->status !== 'pending') {
            return false;
        }

        if (in_array($user->role, ['admin', 'waiter'])) {
            return true;
        }

        return $user->id === $order->user_id;
    }

    public function pay(User $user, Order $order): bool
    {
        if ($order->status !== 'pending') {
            return false;
        }

        if (in_array($user->role, ['admin', 'waiter'])) {
            return true;
        }

        return $user->id === $order->user_id;
    }

    public function close(User $user): bool
    {
        return in_array($user->role, ['admin', 'waiter']);
    }
}
