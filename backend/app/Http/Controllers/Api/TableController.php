<?php

namespace App\Http\Controllers\Api;

use App\Actions\Table\UpdateTableStatusAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Table\UpdateTableStatusRequest;
use App\Models\Table;
use Illuminate\Http\JsonResponse;

class TableController extends Controller
{
    public function index(): JsonResponse
    {
        return response()->json(Table::all(), 200);
    }

    public function show(Table $table): JsonResponse
    {
        return response()->json($table, 200);
    }

    public function updateStatus(
        Table $table,
        UpdateTableStatusRequest $request,
        UpdateTableStatusAction $action
    ): JsonResponse {
        return response()->json($action->execute($table, $request->validated()['status']), 200);
    }
}
