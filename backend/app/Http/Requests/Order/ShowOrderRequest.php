<?php

namespace App\Http\Requests\Order;

use Illuminate\Foundation\Http\FormRequest;

class ShowOrderRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('view', $this->route('order'));
    }

    public function rules(): array
    {
        return [];
    }
}
