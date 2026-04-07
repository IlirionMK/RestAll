<?php

namespace App\Actions\Order;

use App\Models\Order;
use App\Events\UserActionPerformed;

class PayOrderAction
{
    public function execute(Order $order): Order
    {
        $order->update(['status' => 'paid']);

        $order->table()->update(['status' => 'free']);

        event(new UserActionPerformed(
            action: 'order.paid',
            model: $order,
            payload: ['amount' => $order->total_amount]
        ));

        return $order;
    }
}
