<?php

namespace App\Actions\Kitchen;

use App\Enums\OrderItemStatus;
use App\Models\OrderItem;
use Illuminate\Database\Eloquent\Collection;

class ListActiveTicketsAction
{
    public function execute(): Collection
    {
        return OrderItem::with(['order.table'])
            ->whereIn('status', [OrderItemStatus::PENDING, OrderItemStatus::PREPARING])
            ->orderBy('created_at', 'asc')
            ->get();
    }
}
