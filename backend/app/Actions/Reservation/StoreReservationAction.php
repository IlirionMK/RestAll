<?php

namespace App\Actions\Reservation;

use App\Models\Reservation;

class StoreReservationAction
{
    public function execute(array $data, int $userId, int $restaurantId): Reservation
    {
        return Reservation::create([
            ...$data,
            'user_id' => $userId,
            'restaurant_id' => $restaurantId,
            'status' => 'confirmed'
        ]);
    }
}
