<?php

declare(strict_types=1);

namespace App\Http\Requests\Order;

use Illuminate\Foundation\Http\FormRequest;
use OpenApi\Attributes as OA;

#[OA\Schema(
    schema: 'ShowOrderRequest',
    description: 'Request for viewing specific order details'
)]
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
