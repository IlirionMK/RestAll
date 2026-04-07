<?php

namespace App\Http\Requests\Menu;

use Illuminate\Foundation\Http\FormRequest;

class ToggleAvailabilityRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('toggleAvailability', $this->route('menuItem'));
    }

    public function rules(): array
    {
        return [];
    }
}
