<?php

namespace App\Http\Controllers\Api;

use App\Actions\Kitchen\ListActiveTicketsAction;
use App\Actions\Kitchen\UpdateTicketStatusAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Kitchen\ListKitchenTicketsRequest;
use App\Http\Requests\Kitchen\UpdateTicketStatusRequest;
use App\Models\OrderItem;
use Illuminate\Http\JsonResponse;

class KitchenController extends Controller
{
    public function index(ListKitchenTicketsRequest $request, ListActiveTicketsAction $action): JsonResponse
    {
        return response()->json($action->execute(), 200);
    }

    public function updateStatus(
        OrderItem $orderItem,
        UpdateTicketStatusRequest $request,
        UpdateTicketStatusAction $action
    ): JsonResponse {
        return response()->json($action->execute($orderItem, $request->validated()['status']), 200);
    }
}
