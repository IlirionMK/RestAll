<?php

namespace App\Http\Requests\Menu;

use Illuminate\Foundation\Http\FormRequest;

class UpdateMenuItemRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('update', $this->route('menuItem'));
    }

    public function rules(): array
    {
        return [
            'menu_category_id' => ['sometimes', 'exists:menu_categories,id'],
            'name' => ['sometimes', 'string', 'max:255'],
            'price' => ['sometimes', 'numeric', 'min:0'],
            'description' => ['nullable', 'string'],
            'photo_url' => ['nullable', 'string'],
            'is_available' => ['sometimes', 'boolean'],
        ];
    }
}
