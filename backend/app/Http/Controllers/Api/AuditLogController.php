<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Audit\ListAuditLogsRequest;
use App\Actions\Audit\ListAuditLogsAction;
use Illuminate\Http\JsonResponse;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Audit Logs', description: 'System audit and activity logs')]
class AuditLogController extends Controller
{
    #[OA\Get(
        path: '/api/audit-logs',
        summary: 'Get system audit logs',
        security: [['bearerAuth' => []]],
        tags: ['Audit Logs']
    )]
    #[OA\Parameter(
        name: 'limit',
        in: 'query',
        required: false,
        schema: new OA\Schema(type: 'integer', default: 50),
        description: 'Number of records to return'
    )]
    #[OA\Response(
        response: 200,
        description: 'List of audit logs',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 1),
                    new OA\Property(property: 'user_id', type: 'integer', nullable: true, example: 2),
                    new OA\Property(property: 'action', type: 'string', example: 'order_created'),
                    new OA\Property(property: 'description', type: 'string', example: 'Order #15 created at Table #5'),
                    new OA\Property(property: 'created_at', type: 'string', format: 'date-time', example: '2026-04-22T19:30:00Z')
                ]
            )
        )
    )]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    public function index(ListAuditLogsRequest $request, ListAuditLogsAction $action): JsonResponse
    {
        return response()->json($action->execute($request->get('limit', 50)), 200);
    }
}
