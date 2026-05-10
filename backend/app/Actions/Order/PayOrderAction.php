<?php

declare(strict_types=1);

namespace App\Actions\Order;

use App\Enums\OrderStatus;
use App\Enums\TableStatus;
use App\Events\UserActionPerformed;
use App\Models\Order;
use Illuminate\Support\Carbon;

class PayOrderAction
{
    public function execute(Order $order): Order
    {
        $order->update([
            'status' => OrderStatus::PAID,
            'paid_at' => Carbon::now(),
        ]);

        $order->table()->update(['status' => TableStatus::FREE]);

        event(new UserActionPerformed(
            action: 'order.paid',
            model: $order,
            payload: [
                'amount' => $order->total_amount,
                'reservation_id' => $order->reservation_id
            ]
        ));

        return $order;
    }
}
