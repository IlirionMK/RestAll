<?php

declare(strict_types=1);

namespace App\Policies;

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
        if (in_array($user->role, ['admin', 'waiter'])) {
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
        if (in_array($user->role, ['admin', 'waiter'])) {
            return true;
        }

        return $user->id === $reservation->user_id && $reservation->status->value === 'pending';
    }

    public function delete(User $user, Reservation $reservation): bool
    {
        if ($user->role === 'admin') {
            return true;
        }

        return $user->id === $reservation->user_id && $reservation->status->value === 'pending';
    }
}
