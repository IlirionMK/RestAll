<?php

namespace App\Actions\User;

use App\Models\User;
use Illuminate\Support\Facades\Hash;

class StoreStaffAction
{
    public function execute(array $data, int $restaurantId): User
    {
        $data['password'] = Hash::make($data['password']);
        $data['restaurant_id'] = $restaurantId;

        return User::create($data);
    }
}
