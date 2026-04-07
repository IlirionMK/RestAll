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
                'name' => $menuItem->name,
                'price' => $menuItem->price,
                'quantity' => $item['quantity'],
                'status' => 'ordered',
                'comment' => $item['comment'] ?? null,
            ]);

            $order->increment('total_amount', $menuItem->price * $item['quantity']);
        }

        event(new \App\Events\UserActionPerformed(
            action: 'order.items_added',
            model: $order,
            payload: ['items_count' => count($items)]
        ));

        return $order;
    }
}
