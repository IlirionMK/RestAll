<?php

namespace App\Models;

use App\Enums\ReservationStatus;
use App\Traits\BelongsToRestaurant;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class Reservation extends Model
{
    use BelongsToRestaurant;

    protected $fillable = [
        'table_id',
        'user_id',
        'restaurant_id',
        'reservation_time',
        'guests_count',
        'status',
    ];

    protected function casts(): array
    {
        return [
            'reservation_time' => 'datetime',
            'status' => ReservationStatus::class,
        ];
    }

    public function table(): BelongsTo
    {
        return $this->belongsTo(Table::class);
    }

    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class);
    }
}
