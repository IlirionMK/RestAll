<?php

namespace App\Http\Controllers\Api;

use App\Actions\Order\{AddOrderItemsAction,
    CreateOrderAction,
    GetOrderDetailsAction,
    ListActiveOrdersAction,
    PayOrderAction,
    RemoveOrderItemAction};
use App\Http\Controllers\Controller;
use App\Http\Requests\{Order\AddOrderItemsRequest,
    Order\ListOrdersRequest,
    Order\PayOrderRequest,
    Order\RemoveOrderItemRequest,
    Order\ShowOrderRequest,
    Order\StoreOrderRequest};
use App\Models\Order;
use App\Models\OrderItem;
use Illuminate\Http\JsonResponse;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Orders', description: 'Management of restaurant orders and receipts')]
class OrderController extends Controller
{
    #[OA\Get(
        path: '/api/orders',
        summary: 'Get active orders',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of active orders',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 1),
                    new OA\Property(property: 'table_id', type: 'integer', example: 5),
                    new OA\Property(property: 'user_id', type: 'integer', example: 2),
                    new OA\Property(property: 'total_amount', type: 'number', format: 'float', example: 150.00),
                    new OA\Property(property: 'status', type: 'string', example: 'pending')
                ]
            )
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    public function index(ListOrdersRequest $request, ListActiveOrdersAction $action): JsonResponse
    {
        return response()->json($action->execute(), 200);
    }

    #[OA\Post(
        path: '/api/orders',
        summary: 'Create a new empty order',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            required: ['table_id'],
            properties: [
                new OA\Property(property: 'table_id', type: 'integer', example: 5)
            ]
        )
    )]
    #[OA\Response(
        response: 201,
        description: 'Order created successfully',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 1),
                new OA\Property(property: 'table_id', type: 'integer', example: 5),
                new OA\Property(property: 'status', type: 'string', example: 'pending')
            ]
        )
    )]
    #[OA\Response(response: 400, description: 'Bad Request')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    public function store(StoreOrderRequest $request, CreateOrderAction $action): JsonResponse
    {
        return response()->json($action->execute(
            $request->validated(),
            $request->user()->id,
            $request->user()->restaurant_id
        ), 201);
    }

    #[OA\Get(
        path: '/api/orders/{id}',
        summary: 'Get order details',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 1
    )]
    #[OA\Response(
        response: 200,
        description: 'Order details with items',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 1),
                new OA\Property(property: 'total_amount', type: 'number', format: 'float', example: 45.50),
                new OA\Property(
                    property: 'items',
                    type: 'array',
                    items: new OA\Items(
                        properties: [
                            new OA\Property(property: 'id', type: 'integer', example: 10),
                            new OA\Property(property: 'name', type: 'string', example: 'Burger'),
                            new OA\Property(property: 'quantity', type: 'integer', example: 2),
                            new OA\Property(property: 'price', type: 'number', format: 'float', example: 15.00),
                            new OA\Property(property: 'status', type: 'string', example: 'ordered')
                        ]
                    )
                )
            ]
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function show(Order $order, ShowOrderRequest $request, GetOrderDetailsAction $action): JsonResponse
    {
        return response()->json($action->execute($order), 200);
    }

    #[OA\Post(
        path: '/api/orders/{id}/items',
        summary: 'Add items to an order',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
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
                new OA\Property(
                    property: 'items',
                    type: 'array',
                    items: new OA\Items(
                        properties: [
                            new OA\Property(property: 'menu_item_id', type: 'integer', example: 12),
                            new OA\Property(property: 'quantity', type: 'integer', example: 2),
                            new OA\Property(property: 'comment', type: 'string', nullable: true, example: 'No ketchup')
                        ]
                    )
                )
            ]
        )
    )]
    #[OA\Response(response: 201, description: 'Items added successfully')]
    #[OA\Response(response: 400, description: 'Bad Request')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    #[OA\Response(response: 422, description: 'Validation Error')]
    public function addItems(Order $order, AddOrderItemsRequest $request, AddOrderItemsAction $action): JsonResponse
    {
        return response()->json($action->execute($order, $request->validated()['items']), 201);
    }

    #[OA\Delete(
        path: '/api/orders/items/{id}',
        summary: 'Remove an item from an order',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 10
    )]
    #[OA\Response(response: 204, description: 'Item removed successfully')]
    #[OA\Response(response: 400, description: 'Cannot remove item (already cooking)')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function removeItem(OrderItem $orderItem, RemoveOrderItemRequest $request, RemoveOrderItemAction $action): JsonResponse
    {
        $action->execute($orderItem);
        return response()->json(null, 204);
    }

    #[OA\Patch(
        path: '/api/orders/{id}/pay',
        summary: 'Pay and close an order',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 1
    )]
    #[OA\Response(
        response: 200,
        description: 'Order paid successfully',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 1),
                new OA\Property(property: 'status', type: 'string', example: 'paid')
            ]
        )
    )]
    #[OA\Response(response: 400, description: 'Order cannot be paid')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function pay(Order $order, PayOrderRequest $request, PayOrderAction $action): JsonResponse
    {
        return response()->json($action->execute($order), 200);
    }
}
