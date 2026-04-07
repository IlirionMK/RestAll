<?php

namespace App\Actions\Kitchen;

use App\Models\OrderItem;

class UpdateTicketStatusAction
{
    public function execute(OrderItem $orderItem, string $status): OrderItem
    {
        $orderItem->update(['status' => $status]);
        return $orderItem;
    }
}
