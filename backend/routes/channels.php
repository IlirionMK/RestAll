<?php

use Illuminate\Support\Facades\Broadcast;

Broadcast::routes(['middleware' => ['api', 'auth:sanctum']]);

Broadcast::channel('App.Models.User.{id}', function ($user, $id) {
    return (int) $user->id === (int) $id;
});

Broadcast::channel('restaurant.{restaurantId}.staff', function ($user, $restaurantId) {
    return (int) $user->restaurant_id === (int) $restaurantId;
});

Broadcast::channel('restaurant.{restaurantId}.kitchen', function ($user, $restaurantId) {
    return (int) $user->restaurant_id === (int) $restaurantId
        && in_array($user->role->value, ['admin', 'chef'], true);
});
