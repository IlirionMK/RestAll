<?php

namespace App\Http\Requests\Table;

use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Validation\Rule;

class UpdateTableStatusRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('updateStatus', $this->route('table'));
    }

    public function rules(): array
    {
        return [
            'status' => ['required', 'string', Rule::in(['free', 'occupied', 'reserved', 'cleaning'])],
        ];
    }
}
