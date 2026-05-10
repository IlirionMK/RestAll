<?php

declare(strict_types=1);

namespace App\Actions\Order;

use App\Enums\OrderStatus;
use App\Enums\TableStatus;
use App\Events\UserActionPerformed;
use App\Models\Order;
use App\Models\Table;

class CreateOrderAction
{
    public function execute(array $data, int $waiterId, int $restaurantId): Order
    {
        $order = Order::create([
            'table_id' => $data['table_id'],
            'user_id' => $waiterId,
            'restaurant_id' => $restaurantId,
            'reservation_id' => $data['reservation_id'] ?? null,
            'status' => OrderStatus::PENDING,
            'total_amount' => 0,
        ]);

        Table::where('id', $data['table_id'])->update(['status' => TableStatus::OCCUPIED]);

        event(new UserActionPerformed(
            action: 'order.created',
            model: $order,
            payload: [
                'table_id' => $data['table_id'],
                'reservation_id' => $order->reservation_id
            ]
        ));

        return $order;
    }
}
