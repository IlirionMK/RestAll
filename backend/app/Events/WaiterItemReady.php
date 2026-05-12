<?php

declare(strict_types=1);

namespace App\Events;

use App\Models\OrderItem;
use Illuminate\Broadcasting\InteractsWithSockets;
use Illuminate\Broadcasting\PrivateChannel;
use Illuminate\Contracts\Broadcasting\ShouldBroadcast;
use Illuminate\Foundation\Events\Dispatchable;
use Illuminate\Queue\SerializesModels;

class WaiterItemReady implements ShouldBroadcast
{
    use Dispatchable, InteractsWithSockets, SerializesModels;

    public function __construct(
        public OrderItem $orderItem
    ) {}

    public function broadcastOn(): array
    {
        return [
            new PrivateChannel("App.Models.User.{$this->orderItem->order->user_id}"),
        ];
    }

    public function broadcastAs(): string
    {
        return 'item.ready';
    }

    public function broadcastWith(): array
    {
        $this->orderItem->loadMissing(['order.table']);

        return [
            'order_id'     => $this->orderItem->order_id,
            'table_number' => $this->orderItem->order->table?->number,
            'item_name'    => $this->orderItem->name,
            'quantity'     => $this->orderItem->quantity,
        ];
    }
}
