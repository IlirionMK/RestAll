<?php

namespace App\Http\Controllers\Api;

use App\Actions\Reservation\CancelReservationAction;
use App\Actions\Reservation\StoreReservationAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Reservation\StoreReservationRequest;
use App\Models\Reservation;
use Illuminate\Http\JsonResponse;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Reservations', description: 'Table reservations management')]
class ReservationController extends Controller
{
    #[OA\Get(
        path: '/api/reservations',
        summary: 'Get list of reservations for the day',
        security: [['bearerAuth' => []]],
        tags: ['Reservations']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of reservations',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 1),
                    new OA\Property(property: 'table_id', type: 'integer', example: 5),
                    new OA\Property(property: 'user_id', type: 'integer', example: 2),
                    new OA\Property(property: 'reservation_time', type: 'string', format: 'date-time', example: '2026-04-22T19:30:00Z'),
                    new OA\Property(property: 'guests_count', type: 'integer', example: 4),
                    new OA\Property(
                        property: 'table',
                        type: 'object',
                        properties: [
                            new OA\Property(property: 'id', type: 'integer', example: 5),
                            new OA\Property(property: 'number', type: 'string', example: 'T-1')
                        ]
                    )
                ]
            )
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    public function index(): JsonResponse
    {
        $this->authorize('viewAny', Reservation::class);
        return response()->json(Reservation::with(['table', 'user'])->get(), 200);
    }

    #[OA\Post(
        path: '/api/reservations',
        summary: 'Reserve a free table',
        security: [['bearerAuth' => []]],
        tags: ['Reservations']
    )]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            required: ['table_id', 'reservation_time'],
            properties: [
                new OA\Property(property: 'table_id', type: 'integer', example: 5),
                new OA\Property(property: 'reservation_time', type: 'string', format: 'date-time', example: '2026-04-22T19:30:00Z'),
                new OA\Property(property: 'guests_count', type: 'integer', example: 2)
            ]
        )
    )]
    #[OA\Response(
        response: 201,
        description: 'Reservation created successfully',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 1),
                new OA\Property(property: 'table_id', type: 'integer', example: 5),
                new OA\Property(property: 'reservation_time', type: 'string', format: 'date-time', example: '2026-04-22T19:30:00Z')
            ]
        )
    )]
    #[OA\Response(response: 400, description: 'Bad Request')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 409, description: 'Conflict (Table is not available)')]
    #[OA\Response(response: 422, description: 'Validation Error')]
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
        path: '/api/reservations/{id}',
        summary: 'Cancel a reservation',
        security: [['bearerAuth' => []]],
        tags: ['Reservations']
    )]
    #[OA\Parameter(
        name: 'id',
        in: 'path',
        required: true,
        schema: new OA\Schema(type: 'integer'),
        example: 1
    )]
    #[OA\Response(response: 204, description: 'Reservation cancelled successfully')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function destroy(Reservation $reservation, CancelReservationAction $action): JsonResponse
    {
        $this->authorize('delete', $reservation);
        $action->execute($reservation);
        return response()->json(null, 204);
    }
}
