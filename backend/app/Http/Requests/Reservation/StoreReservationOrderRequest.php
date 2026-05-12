<?php

declare(strict_types=1);

namespace App\Http\Requests\Reservation;

use Illuminate\Foundation\Http\FormRequest;
use OpenApi\Attributes as OA;

#[OA\Schema(
    schema: 'StoreReservationOrderRequest',
    properties: [
        new OA\Property(property: 'is_takeaway', type: 'boolean', example: false),
    ]
)]
class StoreReservationOrderRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('createOrder', $this->route('reservation'));
    }

    public function rules(): array
    {
        return [
            'is_takeaway' => ['sometimes', 'boolean'],
        ];
    }
}
