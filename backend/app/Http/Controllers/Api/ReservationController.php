<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Actions\Reservation\CancelReservationAction;
use App\Actions\Reservation\CreateReservationOrderAction;
use App\Actions\Reservation\StoreReservationAction;
use App\Enums\OrderStatus;
use App\Http\Controllers\Controller;
use App\Http\Requests\Reservation\StoreReservationOrderRequest;
use App\Http\Requests\Reservation\StoreReservationRequest;
use App\Models\Order;
use App\Models\Reservation;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Reservations', description: 'Table reservations management')]
class ReservationController extends Controller
{
    #[OA\Get(
        path: '/api/reservations',
        summary: 'Get list of reservations',
        security: [['bearerAuth' => []]],
        tags: ['Reservations']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of reservations',
        content: new OA\JsonContent(type: 'array', items: new OA\Items(
            properties: [
                new OA\Property(property: 'id', type: 'integer'),
                new OA\Property(property: 'table_id', type: 'integer'),
                new OA\Property(property: 'user_id', type: 'integer'),
                new OA\Property(property: 'reservation_time', type: 'string', format: 'date-time'),
                new OA\Property(property: 'guests_count', type: 'integer'),
                new OA\Property(property: 'status', type: 'string'),
            ]
        ))
    )]
    public function index(Request $request): JsonResponse
    {
        $query = Reservation::with(['table', 'restaurant', 'user']);

        if ($request->user()->restaurant_id) {
            $query->where('restaurant_id', $request->user()->restaurant_id);
        } else {
            $query->where('user_id', $request->user()->id);
        }

        return response()->json($query->latest()->get(), 200);
    }

    #[OA\Post(
        path: '/api/reservations',
        summary: 'Reserve a table',
        security: [['bearerAuth' => []]],
        tags: ['Reservations']
    )]
    #[OA\Response(response: 201, description: 'Created')]
    public function store(StoreReservationRequest $request, StoreReservationAction $action): JsonResponse
    {
        $reservation = $action->execute(
            $request->validated(),
            $request->user()->id,
            $request->user()->restaurant_id
        );

        return response()->json($reservation, 201);
    }

    #[OA\Delete(
        path: '/api/reservations/{reservation}',
        summary: 'Cancel a reservation',
        security: [['bearerAuth' => []]],
        tags: ['Reservations']
    )]
    #[OA\Parameter(name: 'reservation', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))]
    #[OA\Response(response: 204, description: 'No Content')]
    public function destroy(Reservation $reservation, CancelReservationAction $action, Request $request): JsonResponse
    {
        $this->authorize('delete', $reservation);
        $action->execute($reservation);
        return response()->json(null, 204);
    }

    #[OA\Post(
        path: '/api/reservations/{reservation}/orders',
        summary: 'Create or retrieve active order for a reservation',
        security: [['bearerAuth' => []]],
        tags: ['Reservations']
    )]
    #[OA\Parameter(name: 'reservation', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))]
    #[OA\Response(response: 201, description: 'Order created or returned')]
    public function createOrder(
        Reservation $reservation,
        StoreReservationOrderRequest $request,
        CreateReservationOrderAction $action
    ): JsonResponse {
        $order = $action->execute(
            $reservation,
            $request->user()->id,
            (bool) $request->input('is_takeaway', false)
        );

        return response()->json($order->load('items.menuItem'), 201);
    }

    #[OA\Get(
        path: '/api/reservations/{reservation}/order',
        summary: 'Get active order for a reservation',
        security: [['bearerAuth' => []]],
        tags: ['Reservations']
    )]
    #[OA\Parameter(name: 'reservation', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))]
    #[OA\Response(response: 200, description: 'Active order or null')]
    public function showOrder(Reservation $reservation, Request $request): JsonResponse
    {
        $this->authorize('viewOrder', $reservation);

        $order = Order::where('reservation_id', $reservation->id)
            ->where('status', '!=', OrderStatus::PAID)
            ->with('items.menuItem')
            ->first();

        return response()->json($order, 200);
    }
}
