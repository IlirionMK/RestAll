<?php

declare(strict_types=1);

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class KitchenTicketResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id'           => $this->id,
            'order_id'     => $this->order_id,
            'table_number' => $this->order?->table?->number,
            'name'         => $this->name,
            'quantity'     => $this->quantity,
            'comment'      => $this->comment,
            'status'       => $this->status->value,
            'created_at'   => $this->created_at->toISOString(),
        ];
    }
}