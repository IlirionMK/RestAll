<?php

declare(strict_types=1);

namespace App\Actions\Order;

use App\Enums\OrderStatus;
use App\Events\OrderBillingRequested;
use App\Events\UserActionPerformed;
use App\Models\Order;

class RequestBillAction
{
    public function execute(Order $order): Order
    {
        $order->update(['status' => OrderStatus::BILLING_REQUESTED]);

        broadcast(new OrderBillingRequested($order->load(['table', 'user'])))->toOthers();

        event(new UserActionPerformed(
            action: 'order.bill_requested',
            model: $order,
            payload: [
                'table_id' => $order->table_id,
                'reservation_id' => $order->reservation_id,
            ]
        ));

        return $order;
    }
}
