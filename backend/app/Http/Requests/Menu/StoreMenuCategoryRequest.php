<?php

namespace App\Http\Requests\Menu;

use App\Models\MenuCategory;
use Illuminate\Foundation\Http\FormRequest;

class StoreMenuCategoryRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('create', MenuCategory::class);
    }

    public function rules(): array
    {
        return [
            'name' => ['required', 'string', 'max:255'],
            'sort_order' => ['nullable', 'integer'],
        ];
    }
}
