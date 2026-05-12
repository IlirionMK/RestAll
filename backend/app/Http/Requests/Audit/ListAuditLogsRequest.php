<?php

namespace App\Http\Requests\Audit;

use App\Enums\UserRole;
use Illuminate\Foundation\Http\FormRequest;

class ListAuditLogsRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->role === UserRole::ADMIN;
    }

    public function rules(): array
    {
        return [
            'limit' => ['sometimes', 'integer', 'max:100'],
        ];
    }
}
