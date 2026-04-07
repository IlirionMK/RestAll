<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\StoreMenuItemRequest;
use App\Actions\Menu\CreateMenuItemAction;
use Illuminate\Http\JsonResponse;

class MenuItemController extends Controller
{
    public function store(StoreMenuItemRequest $request, CreateMenuItemAction $action): JsonResponse
    {
        $menuItem = $action->execute($request->validated());

        return response()->json($menuItem, 201);
    }
}
