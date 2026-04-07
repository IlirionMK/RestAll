<?php

namespace App\Http\Controllers\Api;

use App\Actions\Menu\CreateMenuCategoryAction;
use App\Actions\Menu\UpdateMenuCategoryAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Menu\StoreMenuCategoryRequest;
use App\Http\Requests\Menu\UpdateMenuCategoryRequest;
use App\Models\MenuCategory;
use Illuminate\Http\JsonResponse;

class MenuCategoryController extends Controller
{
    public function index(): JsonResponse
    {
        return response()->json(MenuCategory::with('items')->orderBy('sort_order')->get(), 200);
    }

    public function store(StoreMenuCategoryRequest $request, CreateMenuCategoryAction $action): JsonResponse
    {
        return response()->json($action->execute($request->validated()), 201);
    }

    public function update(
        MenuCategory $menuCategory,
        UpdateMenuCategoryRequest $request,
        UpdateMenuCategoryAction $action
    ): JsonResponse {
        return response()->json($action->execute($menuCategory, $request->validated()), 200);
    }

    public function destroy(MenuCategory $menuCategory): JsonResponse
    {
        $this->authorize('delete', $menuCategory);
        $menuCategory->delete();
        return response()->json(null, 204);
    }
}
