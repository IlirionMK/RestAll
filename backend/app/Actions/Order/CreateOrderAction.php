<?php

namespace App\Actions\Order;

use App\Models\Order;
use App\Models\Table;
use App\Events\UserActionPerformed;

class CreateOrderAction
{
    public function execute(array $data, int $waiterId, int $restaurantId): Order
    {
        $order = Order::create([
            'table_id' => $data['table_id'],
            'user_id' => $waiterId,
            'restaurant_id' => $restaurantId,
            'status' => 'pending',
            'total_price' => 0,
        ]);

        Table::where('id', $data['table_id'])->update(['status' => 'occupied']);

        event(new UserActionPerformed(
            action: 'order.created',
            model: $order,
            payload: ['table_id' => $data['table_id']]
        ));

        return $order;
    }
}
