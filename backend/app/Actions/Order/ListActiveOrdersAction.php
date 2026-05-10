<?php

namespace App\Actions\Order;

use App\Enums\OrderStatus;
use App\Models\Order;
use Illuminate\Database\Eloquent\Collection;

class ListActiveOrdersAction
{
    public function execute(): Collection
    {
        return Order::where('status', '!=', OrderStatus::PAID->value)
            ->with('table')
            ->get();
    }
}
