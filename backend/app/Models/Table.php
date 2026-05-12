<?php

declare(strict_types=1);

namespace App\Models;

use App\Enums\TableStatus;
use App\Traits\BelongsToRestaurant;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Table extends Model
{
    use BelongsToRestaurant, HasFactory;

    protected $fillable = [
        'number',
        'capacity',
        'status',
        'restaurant_id',
    ];

    protected function casts(): array
    {
        return [
            'status' => TableStatus::class,
        ];
    }

    public function reservations(): HasMany
    {
        return $this->hasMany(Reservation::class);
    }

    public function orders(): HasMany
    {
        return $this->hasMany(Order::class);
    }
}
