<?php

namespace App\Http\Requests\Kitchen;

use App\Models\OrderItem;
use Illuminate\Foundation\Http\FormRequest;

class ListKitchenTicketsRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('viewAny', OrderItem::class);
    }

    public function rules(): array
    {
        return [];
    }
}
