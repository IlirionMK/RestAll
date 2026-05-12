<?php

declare(strict_types=1);

namespace App\Models;

use App\Enums\OrderStatus;
use App\Traits\BelongsToRestaurant;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Order extends Model
{
    use BelongsToRestaurant, HasFactory;

    protected $fillable = [
        'restaurant_id',
        'table_id',
        'user_id',
        'reservation_id',
        'total_amount',
        'status',
        'is_takeaway',
        'paid_at',
    ];

    protected function casts(): array
    {
        return [
            'status' => OrderStatus::class,
            'is_takeaway' => 'boolean',
            'paid_at' => 'datetime',
        ];
    }

    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class);
    }

    public function reservation(): BelongsTo
    {
        return $this->belongsTo(Reservation::class);
    }

    public function table(): BelongsTo
    {
        return $this->belongsTo(Table::class);
    }

    public function items(): HasMany
    {
        return $this->hasMany(OrderItem::class);
    }
}
