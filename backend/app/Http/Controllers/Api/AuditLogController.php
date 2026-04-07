<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Audit\ListAuditLogsRequest;
use App\Actions\Audit\ListAuditLogsAction;
use Illuminate\Http\JsonResponse;

class AuditLogController extends Controller
{
    public function index(ListAuditLogsRequest $request, ListAuditLogsAction $action): JsonResponse
    {
        return response()->json($action->execute($request->get('limit', 50)), 200);
    }
}
