<?php

namespace App\Actions\Order;

use App\Models\Order;

class PayOrderAction
{
    public function execute(Order $order): Order
    {
        $order->update(['status' => 'paid']);
        $order->table->update(['status' => 'free']);

        return $order;
    }
}
