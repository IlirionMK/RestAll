<?php

namespace App\Http\Requests\User;

use Illuminate\Foundation\Http\FormRequest;

class Enable2faRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        return [];
    }
}
