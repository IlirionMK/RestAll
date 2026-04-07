<?php

namespace App\Models;

use App\Traits\BelongsToRestaurant;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\Attributes\Fillable;

#[Fillable(['number', 'capacity', 'status', 'restaurant_id'])]
class Table extends Model
{
    use BelongsToRestaurant;

    public function reservations(): HasMany
    {
        return $this->hasMany(Reservation::class);
    }
}
