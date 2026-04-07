<?php

namespace App\Actions\Kitchen;

use App\Models\OrderItem;
use Illuminate\Database\Eloquent\Collection;

class ListActiveTicketsAction
{
    public function execute(): Collection
    {
        return OrderItem::with(['menuItem', 'order.table'])
            ->whereIn('status', ['ordered', 'preparing'])
            ->orderBy('created_at', 'asc')
            ->get();
    }
}
