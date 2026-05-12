<?php

declare(strict_types=1);

namespace App\Events;

use App\Models\Order;
use Illuminate\Broadcasting\InteractsWithSockets;
use Illuminate\Broadcasting\PrivateChannel;
use Illuminate\Contracts\Broadcasting\ShouldBroadcast;
use Illuminate\Foundation\Events\Dispatchable;
use Illuminate\Queue\SerializesModels;

class KitchenOrderItemsAdded implements ShouldBroadcast
{
    use Dispatchable, InteractsWithSockets, SerializesModels;

    public function __construct(
        public Order $order
    ) {}

    public function broadcastOn(): array
    {
        return [
            new PrivateChannel("restaurant.{$this->order->restaurant_id}.kitchen"),
        ];
    }

    public function broadcastAs(): string
    {
        return 'kitchen.items_added';
    }

    public function broadcastWith(): array
    {
        $this->order->loadMissing(['items', 'table']);

        return [
            'order_id' => $this->order->id,
            'table_number' => $this->order->table?->number,
            'items' => $this->order->items
                ->map(fn ($item) => [
                    'id' => $item->id,
                    'name' => $item->name,
                    'quantity' => $item->quantity,
                    'comment' => $item->comment,
                    'status' => $item->status->value,
                ]),
        ];
    }
}
