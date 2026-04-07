<?php

namespace App\Models;

use App\Traits\BelongsToRestaurant;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\Attributes\Fillable;

#[Fillable(['name', 'sort_order', 'restaurant_id'])]
class MenuCategory extends Model
{
    use BelongsToRestaurant;

    public function items(): HasMany
    {
        return $this->hasMany(MenuItem::class);
    }
}
