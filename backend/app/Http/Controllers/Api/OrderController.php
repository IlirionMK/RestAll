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

class OrderController extends Controller
{
    public function index(ListOrdersRequest $request, ListActiveOrdersAction $action): JsonResponse
    {
        return response()->json($action->execute(), 200);
    }

    public function store(StoreOrderRequest $request, CreateOrderAction $action): JsonResponse
    {
        return response()->json($action->execute(
            $request->validated(),
            $request->user()->id,
            $request->user()->restaurant_id
        ), 201);
    }

    public function show(Order $order, ShowOrderRequest $request, GetOrderDetailsAction $action): JsonResponse
    {
        return response()->json($action->execute($order), 200);
    }

    public function addItems(Order $order, AddOrderItemsRequest $request, AddOrderItemsAction $action): JsonResponse
    {
        return response()->json($action->execute($order, $request->validated()['items']), 201);
    }
    public function removeItem(OrderItem $orderItem, RemoveOrderItemRequest $request, RemoveOrderItemAction $action): JsonResponse
    {
        $action->execute($orderItem);
        return response()->json(null, 204);
    }

    public function pay(Order $order, PayOrderRequest $request, PayOrderAction $action): JsonResponse
    {
        return response()->json($action->execute($order), 200);
    }
}
