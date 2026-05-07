<?php

declare(strict_types=1);

namespace App\Actions\Order;

use App\Models\Order;
use App\Events\OrderBillingRequested;
use App\Events\UserActionPerformed;

class RequestBillAction
{
    public function execute(Order $order): Order
    {
        $order->update(['status' => 'billing_requested']);

        broadcast(new OrderBillingRequested($order->load(['table', 'user'])))->toOthers();

        event(new UserActionPerformed(
            action: 'order.bill_requested',
            model: $order,
            payload: [
                'table_id' => $order->table_id,
                'reservation_id' => $order->reservation_id
            ]
        ));

        return $order;
    }
}
