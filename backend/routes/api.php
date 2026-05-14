<?php

declare(strict_types=1);

use App\Http\Controllers\Api\AnalyticsController;
use App\Http\Controllers\Api\AuditLogController;
use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\AuthSessionController;
use App\Http\Controllers\Api\GoogleAuthController;
use App\Http\Controllers\Api\KitchenController;
use App\Http\Controllers\Api\MenuCategoryController;
use App\Http\Controllers\Api\MenuItemController;
use App\Http\Controllers\Api\OrderController;
use App\Http\Controllers\Api\ReservationController;
use App\Http\Controllers\Api\RestaurantController;
use App\Http\Controllers\Api\TableController;
use App\Http\Controllers\Api\UserController;
use Illuminate\Support\Facades\Route;
use Laravel\Fortify\Http\Controllers\AuthenticatedSessionController;
use Laravel\Fortify\Http\Controllers\NewPasswordController;
use Laravel\Fortify\Http\Controllers\PasswordResetLinkController;
use Laravel\Fortify\Http\Controllers\RegisteredUserController;

Route::prefix('auth')->group(function () {
    Route::post('/register', [RegisteredUserController::class, 'store']);
    Route::post('/login', [AuthenticatedSessionController::class, 'store'])->name('login');
    Route::post('/forgot-password', [PasswordResetLinkController::class, 'store']);
    Route::post('/reset-password', [NewPasswordController::class, 'store']);
    Route::post('/token', [AuthSessionController::class, 'store']);

    Route::middleware('web')->group(function () {
        Route::get('/google/redirect', [GoogleAuthController::class, 'redirectToGoogle']);
        Route::get('/google/callback', [GoogleAuthController::class, 'handleGoogleCallback']);
    });
});

Route::get('/restaurants', [RestaurantController::class, 'index']);
Route::get('/restaurants/{restaurant}', [RestaurantController::class, 'show']);
Route::get('/menu/categories', [MenuCategoryController::class, 'index']);

Route::middleware('auth:sanctum')->group(function () {

    Route::prefix('auth')->group(function () {
        Route::post('/logout', [AuthenticatedSessionController::class, 'destroy']);
        Route::delete('/token', [AuthSessionController::class, 'destroy']);
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
