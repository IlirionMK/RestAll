<?php

namespace App\Http\Controllers\Api;

use App\Actions\Menu\CreateMenuItemAction;
use App\Actions\Menu\ToggleMenuItemAvailabilityAction;
use App\Actions\Menu\UpdateMenuItemAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Menu\StoreMenuItemRequest;
use App\Http\Requests\Menu\ToggleAvailabilityRequest;
use App\Http\Requests\Menu\UpdateMenuItemRequest;
use App\Models\MenuItem;
use Illuminate\Http\JsonResponse;

class MenuItemController extends Controller
{
    public function index(): JsonResponse
    {
        return response()->json(MenuItem::with('category')->get());
    }

    public function store(StoreMenuItemRequest $request, CreateMenuItemAction $action): JsonResponse
    {
        return response()->json($action->execute($request->validated()), 201);
    }

    public function update(
        MenuItem $menuItem,
        UpdateMenuItemRequest $request,
        UpdateMenuItemAction $action
    ): JsonResponse {
        return response()->json($action->execute($menuItem, $request->validated()));
    }

    public function toggleAvailability(
        MenuItem $menuItem,
        ToggleAvailabilityRequest $request,
        ToggleMenuItemAvailabilityAction $action
    ): JsonResponse {
        return response()->json($action->execute($menuItem));
    }

    public function destroy(MenuItem $menuItem): JsonResponse
    {
        $menuItem->delete();
        return response()->json(null, 204);
    }
}
