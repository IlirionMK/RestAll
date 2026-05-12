<?php

declare(strict_types=1);

namespace App\Actions\Kitchen;

use App\Enums\OrderItemStatus;
use App\Events\KitchenTicketStatusUpdated;
use App\Models\OrderItem;

class UpdateTicketStatusAction
{
    private const array VALID_TRANSITIONS = [
        'pending'    => ['preparing'],
        'preparing'  => ['ready'],
        'ready'      => ['delivered'],
        'delivered'  => [],
    ];

    public function execute(OrderItem $orderItem, OrderItemStatus $newStatus): OrderItem
    {
        $current = $orderItem->status->value;
        $allowed = self::VALID_TRANSITIONS[$current] ?? [];

        abort_if(
            ! in_array($newStatus->value, $allowed, true),
            422,
            "Invalid transition from [{$current}] to [{$newStatus->value}]."
        );

        $orderItem->update(['status' => $newStatus]);

        broadcast(new KitchenTicketStatusUpdated($orderItem->fresh(['order.table'])));

        return $orderItem;
    }
}