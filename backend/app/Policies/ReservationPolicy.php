<?php

declare(strict_types=1);

namespace App\Policies;

use App\Enums\ReservationStatus;
use App\Enums\UserRole;
use App\Models\Reservation;
use App\Models\User;

class ReservationPolicy
{
    public function viewAny(User $user): bool
    {
        return true;
    }

    public function view(User $user, Reservation $reservation): bool
    {
        if (in_array($user->role, [UserRole::ADMIN, UserRole::WAITER])) {
            return true;
        }

        return $user->id === $reservation->user_id;
    }

    public function create(User $user): bool
    {
        return true;
    }

    public function update(User $user, Reservation $reservation): bool
    {
        if (in_array($user->role, [UserRole::ADMIN, UserRole::WAITER])) {
            return true;
        }

        return $user->id === $reservation->user_id
            && $reservation->status === ReservationStatus::PENDING;
    }

    public function delete(User $user, Reservation $reservation): bool
    {
        if ($user->role === UserRole::ADMIN) {
            return true;
        }

        return $user->id === $reservation->user_id
            && $reservation->status === ReservationStatus::PENDING;
    }

    public function createOrder(User $user, Reservation $reservation): bool
    {
        if (! in_array($reservation->status, [ReservationStatus::PENDING, ReservationStatus::CONFIRMED])) {
            return false;
        }

        return in_array($user->role, [UserRole::ADMIN, UserRole::WAITER]);
    }

    public function viewOrder(User $user, Reservation $reservation): bool
    {
        if (in_array($user->role, [UserRole::ADMIN, UserRole::WAITER])) {
            return true;
        }

        return $user->id === $reservation->user_id;
    }
}
