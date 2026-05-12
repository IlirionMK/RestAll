<?php

declare(strict_types=1);

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class OrderBillResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'order_id'     => $this->id,
            'table_number' => $this->table?->number,
            'status'       => $this->status->value,
            'created_at'   => $this->created_at->toISOString(),
            'paid_at'      => $this->paid_at?->toISOString(),
            'items'        => $this->items->map(fn ($item) => [
                'name'       => $item->name,
                'quantity'   => $item->quantity,
                'unit_price' => (float) $item->price,
                'subtotal'   => (float) ($item->price * $item->quantity),
            ]),
            'total_amount' => (float) $this->total_amount,
        ];
    }
}
