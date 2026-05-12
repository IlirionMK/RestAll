<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Restaurant;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Restaurants', description: 'Restaurant management and discovery')]
class RestaurantController extends Controller
{
    #[OA\Get(
        path: '/api/restaurants',
        summary: 'Get list of all restaurants',
        tags: ['Restaurants']
    )]
    #[OA\Response(
        response: 200,
        description: 'List of restaurants',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(
                properties: [
                    new OA\Property(property: 'id', type: 'integer', example: 1),
                    new OA\Property(property: 'name', type: 'string', example: 'RestAll Premium'),
                    new OA\Property(property: 'address', type: 'string', example: 'Main Avenue, 42'),
                ]
            )
        )
    )]
    public function index(Request $request): JsonResponse
    {
        $query = Restaurant::query();

        if ($request->has('search')) {
            $search = $request->get('search');
            $query->where('name', 'like', "%{$search}%")
                ->orWhere('address', 'like', "%{$search}%");
        }

        return response()->json($query->latest()->get(), 200);
    }

    #[OA\Get(
        path: '/api/restaurants/{restaurant}',
        summary: 'Get restaurant details',
        tags: ['Restaurants']
    )]
    #[OA\Parameter(name: 'restaurant', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))]
    #[OA\Response(response: 200, description: 'Restaurant details')]
    #[OA\Response(response: 404, description: 'Not Found')]
    public function show(Restaurant $restaurant): JsonResponse
    {
        return response()->json($restaurant, 200);
    }
}
