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

class UserController extends Controller
{
    public function me(Request $request): JsonResponse
    {
        return response()->json($request->user(), 200);
    }

    public function updateMe(UpdateMeRequest $request, UpdateUserAction $action): JsonResponse
    {
        return response()->json($action->execute($request->user(), $request->validated()), 200);
    }

    public function requestEmailChange(UpdateEmailRequest $request, UpdateUserAction $action): JsonResponse
    {
        return response()->json($action->execute($request->user(), $request->validated()), 200);
    }

    public function enable2fa(Enable2faRequest $request, UpdateUserAction $action): JsonResponse
    {
        return response()->json($action->execute($request->user(), [
            'two_factor_secret' => encrypt('temporary_secret'),
            'two_factor_recovery_codes' => encrypt(json_encode([])),
        ]), 200);
    }

    public function index(ListUsersRequest $request): JsonResponse
    {
        return response()->json(User::all(), 200);
    }

    public function store(StoreStaffRequest $request, StoreStaffAction $action): JsonResponse
    {
        return response()->json($action->execute($request->validated(), $request->user()->restaurant_id), 201);
    }

    public function updateRole(User $user, UpdateUserRoleRequest $request, UpdateUserAction $action): JsonResponse
    {
        return response()->json($action->execute($user, $request->validated()), 200);
    }

    public function destroy(User $user, DeleteUserRequest $request): JsonResponse
    {
        $user->delete();
        return response()->json(null, 204);
    }
}
