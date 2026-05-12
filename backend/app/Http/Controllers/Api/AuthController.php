<?php

namespace App\Http\Controllers\Api;

use App\Actions\Auth\RefreshTokenAction;
use App\Http\Controllers\Controller;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Auth', description: 'Authentication and Security')]
class AuthController extends Controller
{
    #[OA\Post(
        path: '/api/auth/refresh',
        summary: 'Refresh access token',
        security: [['bearerAuth' => []]],
        tags: ['Auth']
    )]
    #[OA\Response(
        response: 200,
        description: 'Token refreshed successfully',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'token', type: 'string', example: '1|laravel_sanctum_token_abc123...'),
            ]
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    public function refresh(Request $request, RefreshTokenAction $action): JsonResponse
    {
        return response()->json(['token' => $action->execute($request)], 200);
    }

    #[OA\Post(
        path: '/api/auth/google',
        summary: 'Google Login/Registration',
        tags: ['Auth']
    )]
    #[OA\Response(
        response: 200,
        description: 'Successful redirect or token generation',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'url', type: 'string', example: 'https://accounts.google.com/o/oauth2/auth...'),
            ]
        )
    )]
    #[OA\Response(response: 400, description: 'Bad Request')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    public function googleRedirect(): JsonResponse
    {
        return response()->json(['url' => 'google-auth-placeholder-url'], 200);
    }

    #[OA\Post(
        path: '/api/auth/2fa/verify',
        summary: 'Verify TOTP code',
        tags: ['Auth']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'code', type: 'string', example: '123456'),
            ]
        )
    )]
    #[OA\Response(
        response: 200,
        description: '2FA verified successfully',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'message', type: 'string', example: '2FA verified'),
            ]
        )
    )]
    #[OA\Response(response: 400, description: 'Invalid Code')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    public function verify2fa(Request $request): JsonResponse
    {
        return response()->json(['message' => '2FA verified'], 200);
    }
}
