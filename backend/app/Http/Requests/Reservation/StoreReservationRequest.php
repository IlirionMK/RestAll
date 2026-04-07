<?php

namespace App\Http\Requests\Reservation;

use App\Models\Reservation;
use Illuminate\Foundation\Http\FormRequest;

class StoreReservationRequest extends FormRequest
{
    public function authorize(): bool
    {
        return $this->user()->can('create', Reservation::class);
    }

    public function rules(): array
    {
        return [
            'table_id' => ['required', 'exists:tables,id'],
            'reservation_time' => ['required', 'date', 'after:now'],
            'guests_count' => ['required', 'integer', 'min:1'],
        ];
    }
}
