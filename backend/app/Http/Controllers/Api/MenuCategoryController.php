<?php

namespace App\Http\Controllers\Api;

use App\Actions\Menu\CreateMenuCategoryAction;
use App\Actions\Menu\UpdateMenuCategoryAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Menu\StoreMenuCategoryRequest;
use App\Http\Requests\Menu\UpdateMenuCategoryRequest;
use App\Models\MenuCategory;
use Illuminate\Http\JsonResponse;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Menu Categories', description: 'Management of restaurant menu categories')]
class MenuCategoryController extends Controller
{
    #[OA\Get(
        path: '/api/menu/categories',
        summary: 'Get all menu categories with their items',
        tags: ['Menu Categories']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of categories with items',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 1),
                    new OA\Property(property: 'name', type: 'string', example: 'Main Courses'),
                    new OA\Property(property: 'sort_order', type: 'integer', example: 10),
                    new OA\Property(
                        property: 'items',
                        type: 'array',
                        items: new OA\Items(
                            properties: [
                                new OA\Property(property: 'id', type: 'integer', example: 101),
                                new OA\Property(property: 'name', type: 'string', example: 'Steak'),
                                new OA\Property(property: 'price', type: 'number', format: 'float', example: 45.50)
                            ]
                        )
                    )
                ]
            )
        )
    )]
    public function index(): JsonResponse
    {
        return response()->json(MenuCategory::with('items')->orderBy('sort_order')->get(), 200);
    }

    #[OA\Post(
        path: '/api/menu/categories',
        summary: 'Create a new menu category',
        security: [['bearerAuth' => []]],
        tags: ['Menu Categories']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            required: ['name'],
            properties: [
                new OA\Property(property: 'name', type: 'string', example: 'Desserts'),
                new OA\Property(property: 'sort_order', type: 'integer', example: 20)
            ]
        )
    )]
    #[OA\Response(
        response: 201,
        description: 'Category created successfully',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 2),
                new OA\Property(property: 'name', type: 'string', example: 'Desserts'),
                new OA\Property(property: 'sort_order', type: 'integer', example: 20)
            ]
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 422, description: 'Validation Error')]
    public function store(StoreMenuCategoryRequest $request, CreateMenuCategoryAction $action): JsonResponse
    {
        return response()->json($action->execute($request->validated()), 201);
    }

    #[OA\Put(
        path: '/api/menu/categories/{id}',
        summary: 'Update an existing menu category',
        security: [['bearerAuth' => []]],
        tags: ['Menu Categories']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 1
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'name', type: 'string', example: 'Updated Category Name'),
                new OA\Property(property: 'sort_order', type: 'integer', example: 5)
            ]
        )
    )]
    #[OA\Response(
        response: 200,
        description: 'Category updated successfully',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 1),
                new OA\Property(property: 'name', type: 'string', example: 'Updated Category Name'),
                new OA\Property(property: 'sort_order', type: 'integer', example: 5)
            ]
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    #[OA\Response(response: 422, description: 'Validation Error')]
    public function update(
        MenuCategory $menuCategory,
        UpdateMenuCategoryRequest $request,
        UpdateMenuCategoryAction $action
    ): JsonResponse {
        return response()->json($action->execute($menuCategory, $request->validated()), 200);
    }

    #[OA\Delete(
        path: '/api/menu/categories/{id}',
        summary: 'Delete a menu category',
        security: [['bearerAuth' => []]],
        tags: ['Menu Categories']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 1
    )]
    #[OA\Response(response: 204, description: 'Category deleted successfully')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function destroy(MenuCategory $menuCategory): JsonResponse
    {
        $this->authorize('delete', $menuCategory);
        $menuCategory->delete();
        return response()->json(null, 204);
    }
}
