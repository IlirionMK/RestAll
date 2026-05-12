<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Actions\Order\AddOrderItemsAction;
use App\Actions\Order\CreateOrderAction;
use App\Actions\Order\GetOrderDetailsAction;
use App\Actions\Order\GetUserOrderContextAction;
use App\Actions\Order\ListActiveOrdersAction;
use App\Actions\Order\PayOrderAction;
use App\Actions\Order\RemoveOrderItemAction;
use App\Actions\Order\RequestBillAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Order\AddOrderItemsRequest;
use App\Http\Requests\Order\ListOrdersRequest;
use App\Http\Requests\Order\PayOrderRequest;
use App\Http\Requests\Order\RemoveOrderItemRequest;
use App\Http\Requests\Order\RequestBillRequest;
use App\Http\Requests\Order\ShowBillRequest;
use App\Http\Requests\Order\ShowOrderRequest;
use App\Http\Requests\Order\StoreOrderRequest;
use App\Http\Resources\OrderBillResource;
use App\Models\Order;
use App\Models\OrderItem;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Cache;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Orders', description: 'Thin controller orchestrating Order Actions')]
class OrderController extends Controller
{
    #[OA\Get(
        path: '/api/orders/context',
        summary: 'Pre-order context check',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    public function getCurrentContext(GetUserOrderContextAction $action): JsonResponse
    {
        return response()->json($action->execute(), 200);
    }

    #[OA\Get(
        path: '/api/orders',
        summary: 'List active orders',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    public function index(ListOrdersRequest $request, ListActiveOrdersAction $action): JsonResponse
    {
        $userId = $request->user()->id;

        return response()->json(
            Cache::remember("user_{$userId}_orders", 300, fn () => $action->execute()),
            200
        );
    }

    #[OA\Post(
        path: '/api/orders',
        summary: 'Store new order',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    public function store(StoreOrderRequest $request, CreateOrderAction $action): JsonResponse
    {
        $order = $action->execute(
            $request->validated(),
            $request->user()->id,
            $request->user()->restaurant_id
        );
        Cache::forget("user_{$request->user()->id}_orders");

        return response()->json($order, 201);
    }

    #[OA\Get(
        path: '/api/orders/{order}',
        summary: 'Show order details',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    public function show(Order $order, ShowOrderRequest $request, GetOrderDetailsAction $action): JsonResponse
    {
        return response()->json($action->execute($order), 200);
    }

    #[OA\Post(
        path: '/api/orders/{order}/items',
        summary: 'Add items to order',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    public function addItems(Order $order, AddOrderItemsRequest $request, AddOrderItemsAction $action): JsonResponse
    {
        return response()->json($action->execute($order, $request->validated()['items']), 201);
    }

    #[OA\Delete(
        path: '/api/orders/items/{orderItem}',
        summary: 'Remove item from order',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    public function removeItem(OrderItem $orderItem, RemoveOrderItemRequest $request, RemoveOrderItemAction $action): JsonResponse
    {
        $action->execute($orderItem);

        return response()->json(null, 204);
    }

    #[OA\Patch(
        path: '/api/orders/{order}/request-bill',
        summary: 'Request the bill (call waiter)',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    public function requestBill(Order $order, RequestBillRequest $request, RequestBillAction $action): JsonResponse
    {
        $result = $action->execute($order);

        return response()->json($result, 200);
    }

    #[OA\Get(
        path: '/api/orders/{order}/bill',
        summary: 'Get formatted bill for an order',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    #[OA\Response(
        response: 200,
        description: 'Order bill with itemized amounts',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'order_id', type: 'integer'),
                new OA\Property(property: 'table_number', type: 'string', nullable: true),
                new OA\Property(property: 'status', type: 'string'),
                new OA\Property(property: 'total_amount', type: 'number'),
                new OA\Property(property: 'paid_at', type: 'string', nullable: true),
            ]
        )
    )]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function bill(Order $order, ShowBillRequest $request): JsonResponse
    {
        return response()->json(
            new OrderBillResource($order->loadMissing(['items', 'table'])),
            200
        );
    }

    #[OA\Patch(
        path: '/api/orders/{order}/pay',
        summary: 'Pay order (cashier)',
        security: [['bearerAuth' => []]],
        tags: ['Orders']
    )]
    #[OA\Response(response: 200, description: 'Order paid and closed')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function pay(Order $order, PayOrderRequest $request, PayOrderAction $action): JsonResponse
    {
        $result = $action->execute($order);
        Cache::forget("user_{$order->user_id}_orders");

        return response()->json($result, 200);
    }
}
