<?php

namespace App\Actions\Reservation;

use App\Enums\ReservationStatus;
use App\Models\Reservation;
use App\Models\Table;
use Illuminate\Validation\ValidationException;

class StoreReservationAction
{
    public function execute(array $data, int $userId, ?int $restaurantId): Reservation
    {
        $isOccupied = Reservation::where('table_id', $data['table_id'])
            ->where('reservation_time', $data['reservation_time'])
            ->whereIn('status', ReservationStatus::getBusyStatuses())
            ->exists();

        if ($isOccupied) {
            throw ValidationException::withMessages([
                'table_id' => ['The selected table is already reserved or pending confirmation for this time.'],
            ]);
        }

        if (is_null($restaurantId)) {
            $table = Table::findOrFail($data['table_id']);
            $restaurantId = $table->restaurant_id;
        }

        return Reservation::create([
            'table_id' => $data['table_id'],
            'user_id' => $userId,
            'restaurant_id' => $restaurantId,
            'reservation_time' => $data['reservation_time'],
            'guests_count' => $data['guests_count'],
            'status' => ReservationStatus::CONFIRMED,
        ]);
    }
}
