<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Actions\Kitchen\ListActiveTicketsAction;
use App\Actions\Kitchen\UpdateTicketStatusAction;
use App\Enums\OrderItemStatus;
use App\Http\Controllers\Controller;
use App\Http\Requests\Kitchen\ListKitchenTicketsRequest;
use App\Http\Requests\Kitchen\UpdateTicketStatusRequest;
use App\Http\Resources\KitchenTicketResource;
use App\Models\OrderItem;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Kitchen', description: 'Kitchen Display System (KDS)')]
class KitchenController extends Controller
{
    #[OA\Get(
        path: '/api/kitchen/tickets',
        summary: 'Get active kitchen tickets',
        security: [['bearerAuth' => []]],
        tags: ['Kitchen']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of active tickets',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 1),
                    new OA\Property(property: 'order_id', type: 'integer', example: 15),
                    new OA\Property(property: 'table_number', type: 'integer', nullable: true, example: 5),
                    new OA\Property(property: 'name', type: 'string', example: 'Pizza Margherita'),
                    new OA\Property(property: 'quantity', type: 'integer', example: 2),
                    new OA\Property(property: 'comment', type: 'string', nullable: true, example: 'No onions'),
                    new OA\Property(property: 'status', type: 'string', example: 'pending'),
                    new OA\Property(property: 'created_at', type: 'string', format: 'date-time'),
                ]
            )
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    public function index(ListKitchenTicketsRequest $request, ListActiveTicketsAction $action): AnonymousResourceCollection
    {
        return KitchenTicketResource::collection($action->execute());
    }

    #[OA\Patch(
        path: '/api/kitchen/tickets/{id}/status',
        summary: 'Update ticket status',
        security: [['bearerAuth' => []]],
        tags: ['Kitchen']
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
                new OA\Property(property: 'status', type: 'string', enum: ['preparing', 'ready', 'delivered'], example: 'preparing'),
            ]
        )
    )]
    #[OA\Response(response: 200, description: 'Ticket status updated')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    #[OA\Response(response: 422, description: 'Invalid status transition')]
    public function updateStatus(
        OrderItem $orderItem,
        UpdateTicketStatusRequest $request,
        UpdateTicketStatusAction $action
    ): JsonResponse {
        $newStatus = OrderItemStatus::from($request->validated()['status']);

        return response()->json(
            new KitchenTicketResource($action->execute($orderItem, $newStatus)),
            200
        );
    }
}
