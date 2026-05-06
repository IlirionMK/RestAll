<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\Api\{
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

Route::prefix('auth')->group(function () {
    Route::post('/register', [\Laravel\Fortify\Http\Controllers\RegisteredUserController::class, 'store']);
    Route::post('/login', [\Laravel\Fortify\Http\Controllers\AuthenticatedSessionController::class, 'store'])->name('login');
    Route::post('/2fa/challenge', [\Laravel\Fortify\Http\Controllers\TwoFactorAuthenticatedSessionController::class, 'store']);
    Route::post('/forgot-password', [\Laravel\Fortify\Http\Controllers\PasswordResetLinkController::class, 'store']);
    Route::post('/reset-password', [\Laravel\Fortify\Http\Controllers\NewPasswordController::class, 'store']);

    Route::middleware('web')->group(function () {
        Route::get('/google/redirect', [GoogleAuthController::class, 'redirectToGoogle']);
        Route::get('/google/callback', [GoogleAuthController::class, 'handleGoogleCallback']);
    });
});

Route::get('/restaurants', [RestaurantController::class, 'index']);
Route::get('/restaurants/{restaurant}', [RestaurantController::class, 'show']);

Route::middleware('auth:sanctum')->group(function () {
    Route::post('/auth/logout', [\Laravel\Fortify\Http\Controllers\AuthenticatedSessionController::class, 'destroy']);

    Route::prefix('auth/2fa')->group(function () {
        Route::post('/enable', [\Laravel\Fortify\Http\Controllers\TwoFactorAuthenticationController::class, 'store']);
        Route::delete('/disable', [\Laravel\Fortify\Http\Controllers\TwoFactorAuthenticationController::class, 'destroy']);
        Route::get('/qr-code', [\Laravel\Fortify\Http\Controllers\TwoFactorQrCodeController::class, 'show']);
        Route::get('/recovery-codes', [\Laravel\Fortify\Http\Controllers\RecoveryCodeController::class, 'index']);
        Route::post('/confirm', [\Laravel\Fortify\Http\Controllers\ConfirmedTwoFactorAuthenticationController::class, 'store']);
    });

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
