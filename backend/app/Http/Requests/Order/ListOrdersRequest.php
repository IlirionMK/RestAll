<?php

namespace App\Http\Requests\Order;

use App\Models\Order;
use Illuminate\Foundation\Http\FormRequest;

class ListOrdersRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('viewAny', Order::class);
    }

    public function rules(): array
    {
        return [];
    }
}
