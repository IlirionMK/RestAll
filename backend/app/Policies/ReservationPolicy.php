<?php

namespace App\Policies;

use App\Models\Reservation;
use App\Models\User;

class ReservationPolicy
{
    public function viewAny(User $user): bool
    {
        return in_array($user->role, ['admin', 'waiter']);
    }

    public function view(User $user, Reservation $reservation): bool
    {
        return $user->id === $reservation->user_id || in_array($user->role, ['admin', 'waiter']);
    }

    public function create(User $user): bool
    {
        return true;
    }

    public function delete(User $user, Reservation $reservation): bool
    {
        return $user->id === $reservation->user_id || $user->role === 'admin';
    }
}
