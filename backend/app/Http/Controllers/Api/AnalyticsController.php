<?php

declare(strict_types=1);

namespace App\Http\Controllers\Api;

use App\Actions\Analytics\GetAnalyticsSummaryAction;
use App\Http\Controllers\Controller;
use App\Http\Requests\Analytics\AnalyticsSummaryRequest;
use Illuminate\Http\JsonResponse;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Analytics', description: 'Admin analytics endpoints')]
class AnalyticsController extends Controller
{
    #[OA\Get(
        path: '/api/analytics/summary',
        summary: 'Get analytics summary (admin only)',
        security: [['bearerAuth' => []]],
        tags: ['Analytics']
    )]
    #[OA\Response(response: 200, description: 'Analytics summary with revenue, orders, top items, reservations')]
    #[OA\Response(response: 403, description: 'Forbidden')]
    public function summary(AnalyticsSummaryRequest $request, GetAnalyticsSummaryAction $action): JsonResponse
    {
        return response()->json($action->execute(), 200);
    }
}
