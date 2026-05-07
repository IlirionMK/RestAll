<?php

declare(strict_types=1);

namespace App\Events;

use App\Models\Order;
use Illuminate\Broadcasting\InteractsWithSockets;
use Illuminate\Broadcasting\PrivateChannel;
use Illuminate\Contracts\Broadcasting\ShouldBroadcast;
use Illuminate\Foundation\Events\Dispatchable;
use Illuminate\Queue\SerializesModels;

class OrderBillingRequested implements ShouldBroadcast
{
    use Dispatchable, InteractsWithSockets, SerializesModels;

    public function __construct(
        public Order $order
    ) {}

    public function broadcastOn(): array
    {
        return [
            new PrivateChannel("restaurant.{$this->order->restaurant_id}.staff"),
        ];
    }

    public function broadcastAs(): string
    {
        return 'order.billing_requested';
    }

    public function broadcastWith(): array
    {
        return [
            'order_id' => $this->order->id,
            'table_number' => $this->order->table->number ?? 'N/A',
            'total_amount' => $this->order->total_amount,
            'customer_name' => $this->order->user->name,
        ];
    }
}
