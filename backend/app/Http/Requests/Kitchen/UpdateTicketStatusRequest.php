<?php

namespace App\Http\Requests\Kitchen;

use App\Enums\OrderItemStatus;
use App\Models\OrderItem;
use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Validation\Rule;

class UpdateTicketStatusRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('updateStatus', OrderItem::class);
    }

    public function rules(): array
    {
        return [
            'status' => ['required', 'string', Rule::enum(OrderItemStatus::class)],
        ];
    }
}
