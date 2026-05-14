<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Actions\Auth\LoginAction;
use App\Http\Controllers\Controller;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Auth', description: 'Authentication and Security')]
class AuthSessionController extends Controller
{
    #[OA\Post(
        path: '/api/auth/token',
        summary: 'Login with credentials and obtain API token',
        tags: ['Auth']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            required: ['email', 'password'],
            properties: [
                new OA\Property(property: 'email', type: 'string', format: 'email', example: 'staff@restall.com'),
                new OA\Property(property: 'password', type: 'string', example: 'secret'),
                new OA\Property(property: 'device_name', type: 'string', example: 'restall.desktop'),
            ]
        )
    )]
    #[OA\Response(
        response: 200,
        description: 'Token issued',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'token', type: 'string', example: '1|abc123...'),
                new OA\Property(property: 'user', type: 'object'),
            ]
        )
    )]
    #[OA\Response(response: 422, description: 'Invalid credentials')]
    public function store(Request $request, LoginAction $action): JsonResponse
    {
        $validated = $request->validate([
            'email'       => ['required', 'email'],
            'password'    => ['required', 'string'],
            'device_name' => ['sometimes', 'string', 'max:255'],
        ]);

        ['token' => $token, 'user' => $user] = $action->execute(
            $validated['email'],
            $validated['password'],
            $validated['device_name'] ?? 'desktop',
        );

        return response()->json([
            'token' => $token,
            'user'  => [
                'id'            => $user->id,
                'name'          => $user->name,
                'email'         => $user->email,
                'role'          => $user->role->value,
                'restaurant_id' => $user->restaurant_id,
            ],
        ]);
    }

    #[OA\Delete(
        path: '/api/auth/token',
        summary: 'Revoke current API token',
        security: [['bearerAuth' => []]],
        tags: ['Auth']
    )]
    #[OA\Response(response: 200, description: 'Token revoked')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    public function destroy(Request $request): JsonResponse
    {
        $request->user()->currentAccessToken()->delete();

        return response()->json(['message' => 'Token revoked.']);
    }
}
