<?php

declare(strict_types=1);

namespace App\Actions\Reservation;

use App\Enums\OrderStatus;
use App\Enums\TableStatus;
use App\Events\UserActionPerformed;
use App\Models\Order;
use App\Models\Reservation;
use App\Models\Table;

class CreateReservationOrderAction
{
    public function execute(Reservation $reservation, int $userId, bool $isTakeaway): Order
    {
        $existing = Order::where('reservation_id', $reservation->id)
            ->where('status', '!=', OrderStatus::PAID)
            ->first();

        if ($existing) {
            return $existing;
        }

        $order = Order::create([
            'restaurant_id' => $reservation->restaurant_id,
            'table_id' => $reservation->table_id,
            'user_id' => $userId,
            'reservation_id' => $reservation->id,
            'status' => OrderStatus::PENDING,
            'is_takeaway' => $isTakeaway,
            'total_amount' => 0,
        ]);

        if (! $isTakeaway) {
            Table::where('id', $reservation->table_id)->update(['status' => TableStatus::OCCUPIED]);
        }

        event(new UserActionPerformed(
            action: 'order.created_from_reservation',
            model: $order,
            payload: [
                'reservation_id' => $reservation->id,
                'is_takeaway' => $isTakeaway,
            ]
        ));

        return $order;
    }
}
