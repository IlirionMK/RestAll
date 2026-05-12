<?php

declare(strict_types=1);

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\{
    AnalyticsController,
    AuditLogController,
    AuthController,
    GoogleAuthController,
    MenuItemController,
    MenuCategoryController,
    TableController,
    UserController,
    ReservationController,
    OrderController,
    KitchenController,
    RestaurantController
};
use Laravel\Fortify\Http\Controllers\AuthenticatedSessionController;
use Laravel\Fortify\Http\Controllers\RegisteredUserController;
use Laravel\Fortify\Http\Controllers\PasswordResetLinkController;
use Laravel\Fortify\Http\Controllers\NewPasswordController;

Route::prefix('auth')->group(function () {
    Route::post('/register', [RegisteredUserController::class, 'store']);
    Route::post('/login', [AuthenticatedSessionController::class, 'store'])->name('login');
    Route::post('/forgot-password', [PasswordResetLinkController::class, 'store']);
    Route::post('/reset-password', [NewPasswordController::class, 'store']);

    Route::middleware('web')->group(function () {
        Route::get('/google/redirect', [GoogleAuthController::class, 'redirectToGoogle']);
        Route::get('/google/callback', [GoogleAuthController::class, 'handleGoogleCallback']);
    });
});

Route::get('/restaurants', [RestaurantController::class, 'index']);
Route::get('/restaurants/{restaurant}', [RestaurantController::class, 'show']);

Route::middleware('auth:sanctum')->group(function () {

    Route::prefix('auth')->group(function () {
        Route::post('/logout', [AuthenticatedSessionController::class, 'destroy']);
        Route::post('/2fa/verify', [AuthController::class, 'verify2fa']);
        Route::post('/refresh', [AuthController::class, 'refresh']);
    });

    Route::prefix('orders')->group(function () {
        Route::get('/context', [OrderController::class, 'getCurrentContext']);
        Route::get('/', [OrderController::class, 'index']);
        Route::post('/', [OrderController::class, 'store']);
        Route::get('/{order}', [OrderController::class, 'show']);
        Route::get('/{order}/bill', [OrderController::class, 'bill']);
        Route::post('/{order}/items', [OrderController::class, 'addItems']);
        Route::delete('/items/{orderItem}', [OrderController::class, 'removeItem']);
        Route::patch('/{order}/request-bill', [OrderController::class, 'requestBill']);
        Route::patch('/{order}/pay', [OrderController::class, 'pay']);
    });

    Route::prefix('users')->group(function () {
        Route::get('/me', [UserController::class, 'me']);
        Route::put('/me', [UserController::class, 'updateMe']);
        Route::post('/me/email', [UserController::class, 'requestEmailChange']);
        Route::post('/me/2fa/enable', [UserController::class, 'enable2fa']);
        Route::get('/', [UserController::class, 'index']);
        Route::post('/', [UserController::class, 'store']);
        Route::patch('/{user}/role', [UserController::class, 'updateRole']);
        Route::delete('/{user}', [UserController::class, 'destroy']);
    });

    Route::prefix('menu')->group(function () {
        Route::get('/categories', [MenuCategoryController::class, 'index']);
        Route::post('/items', [MenuItemController::class, 'store']);
        Route::put('/items/{menuItem}', [MenuItemController::class, 'update']);
        Route::patch('/items/{menuItem}/availability', [MenuItemController::class, 'toggleAvailability']);
        Route::delete('/items/{menuItem}', [MenuItemController::class, 'destroy']);
    });

    Route::prefix('tables')->group(function () {
        Route::get('/', [TableController::class, 'index']);
        Route::patch('/{table}/status', [TableController::class, 'updateStatus']);
    });

    Route::prefix('reservations')->group(function () {
        Route::get('/', [ReservationController::class, 'index']);
        Route::post('/', [ReservationController::class, 'store']);
        Route::delete('/{reservation}', [ReservationController::class, 'destroy']);
        Route::post('/{reservation}/orders', [ReservationController::class, 'createOrder']);
        Route::get('/{reservation}/order', [ReservationController::class, 'showOrder']);
    });

    Route::prefix('kitchen')->group(function () {
        Route::get('/tickets', [KitchenController::class, 'index']);
        Route::patch('/tickets/{orderItem}/status', [KitchenController::class, 'updateStatus']);
    });

    Route::get('/logs', [AuditLogController::class, 'index']);

    Route::prefix('analytics')->group(function () {
        Route::get('/summary', [AnalyticsController::class, 'summary']);
    });
});
