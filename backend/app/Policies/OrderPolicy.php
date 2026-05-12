<?php

declare(strict_types=1);

namespace App\Policies;

use App\Enums\OrderStatus;
use App\Enums\UserRole;
use App\Models\Order;
use App\Models\User;

class OrderPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [UserRole::ADMIN, UserRole::CASHIER, UserRole::WAITER, UserRole::CHEF]);
    }

    public function view(User $user, Order $order): bool
    {
        if (in_array($user->role, [UserRole::ADMIN, UserRole::CASHIER, UserRole::WAITER, UserRole::CHEF])) {
            return true;
        }

        return $user->id === $order->user_id;
    }

    public function create(User $user): bool
    {
        return in_array($user->role, [UserRole::WAITER, UserRole::GUEST]);
    }

    public function addItems(User $user, Order $order): bool
    {
        if ($order->status !== OrderStatus::PENDING) {
            return false;
        }

        if (in_array($user->role, [UserRole::ADMIN, UserRole::WAITER])) {
            return true;
        }

        return $user->id === $order->user_id;
    }

    public function pay(User $user, Order $order): bool
    {
        if ($order->status !== OrderStatus::PENDING) {
            return false;
        }

        if (in_array($user->role, [UserRole::ADMIN, UserRole::CASHIER, UserRole::WAITER])) {
            return true;
        }

        return $user->id === $order->user_id;
    }

    public function close(User $user): bool
    {
        return in_array($user->role, [UserRole::ADMIN, UserRole::WAITER]);
    }
}
