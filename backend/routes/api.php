<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\{
    AuditLogController,
    AuthController,
    MenuItemController,
    MenuCategoryController,
    TableController,
    UserController,
    ReservationController,
    OrderController,
    KitchenController,
    RestaurantController
};

Route::prefix('auth')->group(function () {
    Route::post('/register', [\Laravel\Fortify\Http\Controllers\RegisteredUserController::class, 'store']);
    Route::post('/login', [\Laravel\Fortify\Http\Controllers\AuthenticatedSessionController::class, 'store'])->name('login');
});

Route::get('/restaurants', [RestaurantController::class, 'index']);
Route::get('/restaurants/{restaurant}', [RestaurantController::class, 'show']);

Route::middleware('auth:sanctum')->group(function () {
    Route::prefix('orders')->group(function () {
        Route::get('/context', [OrderController::class, 'getCurrentContext']);
        Route::get('/', [OrderController::class, 'index']);
        Route::post('/', [OrderController::class, 'store']);
        Route::get('/{order}', [OrderController::class, 'show']);
        Route::post('/{order}/items', [OrderController::class, 'addItems']);
        Route::delete('/items/{orderItem}', [OrderController::class, 'removeItem']);
        Route::patch('/{order}/pay', [OrderController::class, 'pay']);
    });

    Route::prefix('users')->group(function () {
        Route::get('/me', [UserController::class, 'me']);
        Route::get('/', [UserController::class, 'index']);
    });

    Route::prefix('menu')->group(function () {
        Route::get('/categories', [MenuCategoryController::class, 'index']);
        Route::post('/items', [MenuItemController::class, 'store']);
    });

    Route::prefix('reservations')->group(function () {
        Route::get('/', [ReservationController::class, 'index']);
        Route::post('/', [ReservationController::class, 'store']);
        Route::delete('/{reservation}', [ReservationController::class, 'destroy']);
    });

    Route::get('/logs', [AuditLogController::class, 'index']);
});
