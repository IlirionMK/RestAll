<?php

namespace App\Actions\Reservation;

use App\Models\Reservation;

class CancelReservationAction
{
    public function execute(Reservation $reservation): bool
    {
        return $reservation->delete();
    }
}
