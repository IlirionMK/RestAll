<?php

namespace App\Http\Controllers\Api;

use App\Actions\Reservation\CancelReservationAction;
use App\Actions\Reservation\StoreReservationAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Reservation\StoreReservationRequest;
use App\Models\Reservation;
use Illuminate\Http\JsonResponse;

class ReservationController extends Controller
{
    public function index(): JsonResponse
    {
        $this->authorize('viewAny', Reservation::class);
        return response()->json(Reservation::with(['table', 'user'])->get(), 200);
    }

    public function store(StoreReservationRequest $request, StoreReservationAction $action): JsonResponse
    {
        $reservation = $action->execute(
            $request->validated(),
            $request->user()->id,
            $request->user()->restaurant_id
        );

        return response()->json($reservation, 201);
    }

    public function destroy(Reservation $reservation, CancelReservationAction $action): JsonResponse
    {
        $this->authorize('delete', $reservation);
        $action->execute($reservation);
        return response()->json(null, 204);
    }
}
