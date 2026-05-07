<?php

declare(strict_types=1);

namespace App\Http\Requests\Order;

use App\Models\Order;
use Illuminate\Foundation\Http\FormRequest;
use OpenApi\Attributes as OA;

#[OA\Schema(
    schema: 'StoreOrderRequest',
    required: ['table_id'],
    properties: [
        new OA\Property(property: 'table_id', type: 'integer', example: 1),
        new OA\Property(property: 'reservation_id', type: 'integer', nullable: true, example: 10)
    ]
)]
class StoreOrderRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('create', Order::class);
    }

    public function rules(): array
    {
        return [
            'table_id' => ['required', 'integer', 'exists:tables,id'],
            'reservation_id' => ['nullable', 'integer', 'exists:reservations,id'],
        ];
    }
}
