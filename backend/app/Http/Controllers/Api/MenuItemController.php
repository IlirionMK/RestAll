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
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Menu Items', description: 'Management of restaurant menu items')]
class MenuItemController extends Controller
{
    #[OA\Get(
        path: '/api/menu/items',
        summary: 'Get all menu items',
        tags: ['Menu Items']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of menu items',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 1),
                    new OA\Property(property: 'menu_category_id', type: 'integer', example: 2),
                    new OA\Property(property: 'name', type: 'string', example: 'Caesar Salad'),
                    new OA\Property(property: 'description', type: 'string', nullable: true, example: 'Classic salad with croutons'),
                    new OA\Property(property: 'price', type: 'number', format: 'float', example: 12.50),
                    new OA\Property(property: 'is_available', type: 'boolean', example: true),
                    new OA\Property(
                        property: 'category',
                        type: 'object',
                        properties: [
                            new OA\Property(property: 'id', type: 'integer', example: 2),
                            new OA\Property(property: 'name', type: 'string', example: 'Salads'),
                        ]
                    ),
                ]
            )
        )
    )]
    public function index(): JsonResponse
    {
        return response()->json(MenuItem::with('category')->get());
    }

    #[OA\Post(
        path: '/api/menu/items',
        summary: 'Create a new menu item',
        security: [['bearerAuth' => []]],
        tags: ['Menu Items']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            required: ['menu_category_id', 'name', 'price'],
            properties: [
                new OA\Property(property: 'menu_category_id', type: 'integer', example: 2),
                new OA\Property(property: 'name', type: 'string', example: 'Greek Salad'),
                new OA\Property(property: 'description', type: 'string', example: 'Fresh vegetables with feta cheese'),
                new OA\Property(property: 'price', type: 'number', format: 'float', example: 10.00),
                new OA\Property(property: 'is_available', type: 'boolean', example: true),
            ]
        )
    )]
    #[OA\Response(
        response: 201,
        description: 'Menu item created successfully'
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 422, description: 'Validation Error')]
    public function store(StoreMenuItemRequest $request, CreateMenuItemAction $action): JsonResponse
    {
        return response()->json($action->execute($request->validated()), 201);
    }

    #[OA\Put(
        path: '/api/menu/items/{id}',
        summary: 'Update an existing menu item',
        security: [['bearerAuth' => []]],
        tags: ['Menu Items']
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
                new OA\Property(property: 'menu_category_id', type: 'integer', example: 2),
                new OA\Property(property: 'name', type: 'string', example: 'Greek Salad Updated'),
                new OA\Property(property: 'description', type: 'string', example: 'Fresh vegetables with extra feta'),
                new OA\Property(property: 'price', type: 'number', format: 'float', example: 11.50),
            ]
        )
    )]
    #[OA\Response(response: 200, description: 'Menu item updated successfully')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    #[OA\Response(response: 422, description: 'Validation Error')]
    public function update(
        MenuItem $menuItem,
        UpdateMenuItemRequest $request,
        UpdateMenuItemAction $action
    ): JsonResponse {
        return response()->json($action->execute($menuItem, $request->validated()));
    }

    #[OA\Patch(
        path: '/api/menu/items/{id}/availability',
        summary: 'Toggle menu item availability (stop-list)',
        security: [['bearerAuth' => []]],
        tags: ['Menu Items']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 1
    )]
    #[OA\Response(response: 200, description: 'Availability toggled successfully')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function toggleAvailability(
        MenuItem $menuItem,
        ToggleAvailabilityRequest $request,
        ToggleMenuItemAvailabilityAction $action
    ): JsonResponse {
        return response()->json($action->execute($menuItem));
    }

    #[OA\Delete(
        path: '/api/menu/items/{id}',
        summary: 'Delete a menu item',
        security: [['bearerAuth' => []]],
        tags: ['Menu Items']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 1
    )]
    #[OA\Response(response: 204, description: 'Menu item deleted successfully')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function destroy(MenuItem $menuItem): JsonResponse
    {
        $menuItem->delete();

        return response()->json(null, 204);
    }
}
