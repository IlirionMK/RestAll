<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Actions\Auth\RefreshTokenAction;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class AuthController extends Controller
{
    public function refresh(Request $request, RefreshTokenAction $action): JsonResponse
    {
        return response()->json(['token' => $action->execute($request)], 200);
    }

    public function googleRedirect(): JsonResponse
    {
        return response()->json(['url' => 'google-auth-placeholder-url'], 200);
    }

    public function verify2fa(Request $request): JsonResponse
    {
        return response()->json(['message' => '2FA verified'], 200);
    }
}
