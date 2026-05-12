<?php

declare(strict_types=1);

namespace App\Models;

use App\Traits\BelongsToRestaurant;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class MenuItem extends Model
{
    use BelongsToRestaurant, HasFactory;

    protected $fillable = [
        'menu_category_id',
        'name',
        'description',
        'price',
        'photo_url',
        'is_available',
        'restaurant_id',
    ];

    protected function casts(): array
    {
        return [
            'price' => 'decimal:2',
            'is_available' => 'boolean',
        ];
    }

    public function category(): BelongsTo
    {
        return $this->belongsTo(MenuCategory::class);
    }

    public function orderItems(): HasMany
    {
        return $this->hasMany(OrderItem::class);
    }
}
