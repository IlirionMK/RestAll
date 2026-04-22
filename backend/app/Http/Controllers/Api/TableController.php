<?php

namespace App\Http\Controllers\Api;

use App\Actions\Table\UpdateTableStatusAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Table\UpdateTableStatusRequest;
use App\Models\Table;
use Illuminate\Http\JsonResponse;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Tables', description: 'Restaurant tables management')]
class TableController extends Controller
{
    #[OA\Get(
        path: '/api/tables',
        summary: 'Get all tables',
        security: [['bearerAuth' => []]],
        tags: ['Tables']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of all tables',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 1),
                    new OA\Property(property: 'restaurant_id', type: 'integer', example: 1),
                    new OA\Property(property: 'number', type: 'string', example: 'T-1'),
                    new OA\Property(property: 'capacity', type: 'integer', example: 4),
                    new OA\Property(property: 'status', type: 'string', example: 'free')
                ]
            )
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    public function index(): JsonResponse
    {
        return response()->json(Table::all(), 200);
    }

    #[OA\Get(
        path: '/api/tables/{id}',
        summary: 'Get table details',
        security: [['bearerAuth' => []]],
        tags: ['Tables']
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
        description: 'Table details',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 1),
                new OA\Property(property: 'restaurant_id', type: 'integer', example: 1),
                new OA\Property(property: 'number', type: 'string', example: 'T-1'),
                new OA\Property(property: 'capacity', type: 'integer', example: 4),
                new OA\Property(property: 'status', type: 'string', example: 'free')
            ]
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function show(Table $table): JsonResponse
    {
        return response()->json($table, 200);
    }

    #[OA\Patch(
        path: '/api/tables/{id}/status',
        summary: 'Update table status',
        security: [['bearerAuth' => []]],
        tags: ['Tables']
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
            required: ['status'],
            properties: [
                new OA\Property(property: 'status', type: 'string', example: 'cleaning')
            ]
        )
    )]
    #[OA\Response(
        response: 200,
        description: 'Table status updated successfully',
        content: new OA\JsonContent(
            properties: [
                new OA\Property(property: 'id', type: 'integer', example: 1),
                new OA\Property(property: 'status', type: 'string', example: 'cleaning')
            ]
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    #[OA\Response(response: 404, description: 'Not Found')]
    #[OA\Response(response: 422, description: 'Validation Error')]
    public function updateStatus(
        Table $table,
        UpdateTableStatusRequest $request,
        UpdateTableStatusAction $action
    ): JsonResponse {
        return response()->json($action->execute($table, $request->validated()['status']), 200);
    }
}
