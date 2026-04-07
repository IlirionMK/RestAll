<?php

namespace App\Http\Requests\Order;

use Illuminate\Foundation\Http\FormRequest;

class RemoveOrderItemRequest extends FormRequest
{
    public function authorize(): bool
    {
        $orderItem = $this->route('orderItem');
        return $this->user()->can('delete', $orderItem) && $orderItem->status === 'ordered';
    }

    public function rules(): array
    {
        return [];
    }
}
