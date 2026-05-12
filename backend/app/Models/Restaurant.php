<?php

declare(strict_types=1);

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Restaurant extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'address',
    ];

    public function users(): HasMany
    {
        return $this->hasMany(User::class);
    }

    public function tables(): HasMany
    {
        return $this->hasMany(Table::class);
    }

    public function menuCategories(): HasMany
    {
        return $this->hasMany(MenuCategory::class);
    }
}
