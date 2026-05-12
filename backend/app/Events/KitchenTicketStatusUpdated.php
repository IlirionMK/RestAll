<?php

declare(strict_types=1);

namespace App\Events;

use App\Models\OrderItem;
use Illuminate\Broadcasting\InteractsWithSockets;
use Illuminate\Broadcasting\PrivateChannel;
use Illuminate\Contracts\Broadcasting\ShouldBroadcast;
use Illuminate\Foundation\Events\Dispatchable;
use Illuminate\Queue\SerializesModels;

class KitchenTicketStatusUpdated implements ShouldBroadcast
{
    use Dispatchable, InteractsWithSockets, SerializesModels;

    public function __construct(
        public OrderItem $orderItem
    ) {}

    public function broadcastOn(): array
    {
        $restaurantId = $this->orderItem->order->restaurant_id;

        return [
            new PrivateChannel("restaurant.{$restaurantId}.kitchen"),
        ];
    }

    public function broadcastAs(): string
    {
        return 'kitchen.ticket_status_updated';
    }

    public function broadcastWith(): array
    {
        $this->orderItem->loadMissing(['order.table']);

        return [
            'id' => $this->orderItem->id,
            'order_id' => $this->orderItem->order_id,
            'table_number' => $this->orderItem->order->table?->number,
            'name' => $this->orderItem->name,
            'quantity' => $this->orderItem->quantity,
            'status' => $this->orderItem->status->value,
        ];
    }
}
