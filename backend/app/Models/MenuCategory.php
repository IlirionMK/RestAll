<?php

declare(strict_types=1);

namespace App\Models;

use App\Traits\BelongsToRestaurant;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class MenuCategory extends Model
{
    use BelongsToRestaurant, HasFactory;

    protected $fillable = [
        'name',
        'sort_order',
        'restaurant_id',
    ];

    public function items(): HasMany
    {
        return $this->hasMany(MenuItem::class);
    }
}