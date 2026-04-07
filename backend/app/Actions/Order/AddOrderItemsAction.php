<?php

namespace App\Actions\Order;

use App\Models\Order;
use App\Models\MenuItem;

class AddOrderItemsAction
{
    public function execute(Order $order, array $items): Order
    {
        foreach ($items as $item) {
            $menuItem = MenuItem::find($item['menu_item_id']);

            $order->items()->create([
                'menu_item_id' => $item['menu_item_id'],
                'quantity' => $item['quantity'],
                'price_at_order' => $menuItem->price,
                'status' => 'ordered',
                'comment' => $item['comment'] ?? null,
            ]);

            $order->increment('total_price', $menuItem->price * $item['quantity']);
        }

        return $order;
    }
}
