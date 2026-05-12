<?php

namespace App\Actions\Reservation;

use App\Enums\ReservationStatus;
use App\Models\Reservation;
use Illuminate\Validation\ValidationException;

class CancelReservationAction
{
    public function execute(Reservation $reservation): void
    {
        if ($reservation->status === ReservationStatus::COMPLETED) {
            throw ValidationException::withMessages([
                'status' => ['Cannot cancel a completed reservation.'],
            ]);
        }

        $reservation->update([
            'status' => ReservationStatus::CANCELLED,
        ]);
    }
}
