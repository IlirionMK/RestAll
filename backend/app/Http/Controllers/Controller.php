<?php

namespace App\Http\Controllers;

use OpenApi\Attributes as OA;

#[OA\Info(
    version: '1.0.0',
    title: 'RestAll API',
    description: 'REST API documentation for the RestAll restaurant management system.'
)]
#[OA\SecurityScheme(
    securityScheme: 'bearerAuth',
    type: 'http',
    scheme: 'bearer',
    description: 'Enter the token obtained during login'
)]
abstract class Controller
{
    use \Illuminate\Foundation\Auth\Access\AuthorizesRequests;
}
