<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Actions\Audit\ListAuditLogsAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Audit\ListAuditLogsRequest;
use Illuminate\Http\JsonResponse;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Audit Logs', description: 'System audit and activity logs')]
class AuditLogController extends Controller
{
    #[OA\Get(
        path: '/api/logs',
        summary: 'Get system audit logs (admin only)',
        security: [['bearerAuth' => []]],
        tags: ['Audit Logs']
    )]
    #[OA\Parameter(name: 'action', in: 'query', required: false, schema: new OA\Schema(type: 'string'))]
    #[OA\Parameter(name: 'user_id', in: 'query', required: false, schema: new OA\Schema(type: 'integer'))]
    #[OA\Parameter(name: 'date_from', in: 'query', required: false, schema: new OA\Schema(type: 'string', format: 'date'))]
    #[OA\Parameter(name: 'date_to', in: 'query', required: false, schema: new OA\Schema(type: 'string', format: 'date'))]
    #[OA\Parameter(name: 'per_page', in: 'query', required: false, schema: new OA\Schema(type: 'integer', default: 50))]
    #[OA\Response(response: 200, description: 'Paginated list of audit logs')]
    #[OA\Response(response: 401, description: 'Unauthorized')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    public function index(ListAuditLogsRequest $request, ListAuditLogsAction $action): JsonResponse
    {
        return response()->json($action->execute($request->validated()), 200);
    }
}
