<?php

namespace App\Traits;

use App\Models\Restaurant;
use Illuminate\Database\Eloquent\Builder;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Support\Facades\Auth;

trait BelongsToRestaurant
{
    protected static function bootBelongsToRestaurant(): void
    {
        static::addGlobalScope('restaurant', function (Builder $builder) {
            if (Auth::check() && Auth::user()->restaurant_id) {
                $builder->where('restaurant_id', Auth::user()->restaurant_id);
            }
        });

        static::creating(function ($model) {
            if (Auth::check() && Auth::user()->restaurant_id && empty($model->restaurant_id)) {
                $model->restaurant_id = Auth::user()->restaurant_id;
            }
        });
    }

    public function restaurant(): BelongsTo
    {
        return $this->belongsTo(Restaurant::class);
    }
}
