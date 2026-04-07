<?php

namespace App\Models;

use App\Traits\BelongsToRestaurant;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Attributes\Fillable;

#[Fillable(['table_id', 'user_id', 'reservation_time', 'guests_count', 'status', 'restaurant_id'])]
class Reservation extends Model
{
    use BelongsToRestaurant;

    protected function casts(): array
    {
        return [
            'reservation_time' => 'datetime',
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
