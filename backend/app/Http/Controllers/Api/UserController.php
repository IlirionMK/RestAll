<?php

namespace App\Http\Controllers\Api;

use App\Actions\User\StoreStaffAction;
use App\Actions\User\UpdateUserAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\User\DeleteUserRequest;
use App\Http\Requests\User\Enable2faRequest;
use App\Http\Requests\User\ListUsersRequest;
use App\Http\Requests\User\StoreStaffRequest;
use App\Http\Requests\User\UpdateEmailRequest;
use App\Http\Requests\User\UpdateMeRequest;
use App\Http\Requests\User\UpdateUserRoleRequest;
use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Users', description: 'User profile and staff management')]
class UserController extends Controller
{
    #[OA\Get(
        path: '/api/users/me',
        summary: 'Get current user profile',
        security: [['bearerAuth' => []]],
        tags: ['Users']
    )]
    #[OA\Response(
        response: 200,
        description: 'User profile details',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 1),
                new OA\Property(property: 'name', type: 'string', example: 'John Doe'),
                new OA\Property(property: 'email', type: 'string', example: 'john@example.com'),
                new OA\Property(property: 'role', type: 'string', example: 'waiter'),
                new OA\Property(property: 'restaurant_id', type: 'integer', example: 1),
            ]
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    public function me(Request $request): JsonResponse
    {
        return response()->json($request->user(), 200);
    }

    #[OA\Put(
        path: '/api/users/me',
        summary: 'Update current user profile',
        security: [['bearerAuth' => []]],
        tags: ['Users']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'name', type: 'string', example: 'John Smith'),
                new OA\Property(property: 'phone', type: 'string', example: '+123456789'),
            ]
        )
    )]
    #[OA\Response(response: 200, description: 'Profile updated successfully')]
    #[OA\Response(response: 400, description: 'Bad Request')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 422, description: 'Validation Error')]
    public function updateMe(UpdateMeRequest $request, UpdateUserAction $action): JsonResponse
    {
        return response()->json($action->execute($request->user(), $request->validated()), 200);
    }

    #[OA\Post(
        path: '/api/users/me/email',
        summary: 'Request email change',
        security: [['bearerAuth' => []]],
        tags: ['Users']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'email', type: 'string', example: 'new_email@example.com'),
            ]
        )
    )]
    #[OA\Response(response: 200, description: 'Email change requested')]
    #[OA\Response(response: 400, description: 'Bad Request')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 409, description: 'Conflict')]
    public function requestEmailChange(UpdateEmailRequest $request, UpdateUserAction $action): JsonResponse
    {
        return response()->json($action->execute($request->user(), $request->validated()), 200);
    }

    #[OA\Post(
        path: '/api/users/me/2fa/enable',
        summary: 'Enable Two-Factor Authentication',
        security: [['bearerAuth' => []]],
        tags: ['Users']
    )]
    #[OA\Response(response: 200, description: '2FA enabled successfully')]
    #[OA\Response(response: 400, description: 'Bad Request')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    public function enable2fa(Enable2faRequest $request, UpdateUserAction $action): JsonResponse
    {
        return response()->json($action->execute($request->user(), [
            'two_factor_secret' => encrypt('temporary_secret'),
            'two_factor_recovery_codes' => encrypt(json_encode([])),
        ]), 200);
    }

    #[OA\Get(
        path: '/api/users',
        summary: 'List all staff members',
        security: [['bearerAuth' => []]],
        tags: ['Users']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of users',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 2),
                    new OA\Property(property: 'name', type: 'string', example: 'Jane Smith'),
                    new OA\Property(property: 'email', type: 'string', example: 'jane@example.com'),
                    new OA\Property(property: 'role', type: 'string', example: 'chef'),
                ]
            )
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    public function index(ListUsersRequest $request): JsonResponse
    {
        return response()->json(User::all(), 200);
    }

    #[OA\Post(
        path: '/api/users',
        summary: 'Create a new staff member',
        security: [['bearerAuth' => []]],
        tags: ['Users']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            required: ['name', 'email', 'password', 'role'],
            properties: [
                new OA\Property(property: 'name', type: 'string', example: 'Mike Chef'),
                new OA\Property(property: 'email', type: 'string', example: 'mike@example.com'),
                new OA\Property(property: 'password', type: 'string', example: 'secret123'),
                new OA\Property(property: 'role', type: 'string', example: 'chef'),
            ]
        )
    )]
    #[OA\Response(response: 201, description: 'Staff member created successfully')]
    #[OA\Response(response: 400, description: 'Bad Request')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 409, description: 'Conflict')]
    public function store(StoreStaffRequest $request, StoreStaffAction $action): JsonResponse
    {
        return response()->json($action->execute($request->validated(), $request->user()->restaurant_id), 201);
    }

    #[OA\Patch(
        path: '/api/users/{id}/role',
        summary: 'Update staff member role',
        security: [['bearerAuth' => []]],
        tags: ['Users']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 2
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            required: ['role'],
            properties: [
                new OA\Property(property: 'role', type: 'string', example: 'admin'),
            ]
        )
    )]
    #[OA\Response(response: 200, description: 'Role updated successfully')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function updateRole(User $user, UpdateUserRoleRequest $request, UpdateUserAction $action): JsonResponse
    {
        return response()->json($action->execute($user, $request->validated()), 200);
    }

    #[OA\Delete(
        path: '/api/users/{id}',
        summary: 'Block a staff member (Soft Delete)',
        security: [['bearerAuth' => []]],
        tags: ['Users']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 2
    )]
    #[OA\Response(response: 204, description: 'User deleted successfully')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function destroy(User $user, DeleteUserRequest $request): JsonResponse
    {
        $user->delete();

        return response()->json(null, 204);
    }
}
