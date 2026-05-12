<?php

declare(strict_types=1);

namespace App\Http\Requests\Analytics;

use App\Enums\UserRole;
use Illuminate\Foundation\Http\FormRequest;

class AnalyticsSummaryRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->role === UserRole::ADMIN;
    }

    public function rules(): array
    {
        return [];
    }
}
