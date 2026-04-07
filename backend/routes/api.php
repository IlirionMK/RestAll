<?php

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\MenuItemController;

Route::prefix('menu')->group(function () {
    Route::post('/items', [MenuItemController::class, 'store']);
});

Route::get('/user', function (Request $request) {
    return $request->user();
})->middleware('auth:sanctum');
