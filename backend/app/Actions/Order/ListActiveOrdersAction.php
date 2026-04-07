<?php

namespace App\Actions\Order;

use App\Models\Order;
use Illuminate\Database\Eloquent\Collection;

class ListActiveOrdersAction
{
    public function execute(): Collection
    {
        return Order::where('status', '!=', 'paid')
            ->with('table')
            ->get();
    }
}
