<?php

namespace App\Actions\Order;

use App\Models\Order;

class GetOrderDetailsAction
{
    public function execute(Order $order): Order
    {
        return $order->load('items.menuItem');
    }
}
