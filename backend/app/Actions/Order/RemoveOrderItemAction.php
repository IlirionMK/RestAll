<?php

namespace App\Actions\Order;

use App\Models\OrderItem;
use Illuminate\Support\Facades\DB;

class RemoveOrderItemAction
{
    public function execute(OrderItem $orderItem): void
    {
        DB::transaction(function () use ($orderItem) {
            $order = $orderItem->order;
            $order->decrement('total_price', $orderItem->price_at_order * $orderItem->quantity);
            $orderItem->delete();
        });
    }
}
