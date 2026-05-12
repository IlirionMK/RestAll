<?php

namespace App\Actions\Order;

use App\Enums\ReservationStatus;
use App\Models\Reservation;
use Carbon\Carbon;
use Illuminate\Support\Facades\Auth;

class GetUserOrderContextAction
{
    public function execute(): array
    {
        $user = Auth::user();

        $activeReservation = Reservation::where('user_id', $user->id)
            ->where('status', ReservationStatus::CONFIRMED)
            ->whereBetween('reservation_time', [
                Carbon::now()->subHours(2),
                Carbon::now()->addHours(2),
            ])
            ->with(['table', 'restaurant'])
            ->first();

        if (! $activeReservation) {
            return [
                'has_active_context' => false,
                'reservation' => null,
            ];
        }

        return [
            'has_active_context' => true,
            'reservation' => [
                'id' => $activeReservation->id,
                'table_id' => $activeReservation->table_id,
                'table_number' => $activeReservation->table->number ?? $activeReservation->table_id,
                'restaurant_id' => $activeReservation->restaurant_id,
                'restaurant_name' => $activeReservation->restaurant->name,
            ],
        ];
    }
}
