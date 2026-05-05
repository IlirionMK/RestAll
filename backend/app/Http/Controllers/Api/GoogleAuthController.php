<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Laravel\Socialite\Facades\Socialite;
use App\Models\User;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Str;
use OpenApi\Attributes as OA;
use Symfony\Component\HttpFoundation\RedirectResponse;

#[OA\Tag(name: 'Auth', description: 'Authentication and authorization')]
class GoogleAuthController extends Controller
{
    #[OA\Get(
        path: '/api/auth/google/redirect',
        summary: 'Redirect to Google OAuth provider',
        tags: ['Auth']
    )]
    #[OA\Response(
        response: 302,
        description: 'Redirects to Google authentication consent screen'
    )]
    public function redirectToGoogle(): RedirectResponse
    {
        return Socialite::driver('google')->stateless()->redirect();
    }

    #[OA\Get(
        path: '/api/auth/google/callback',
        summary: 'Handle Google OAuth callback',
        tags: ['Auth']
    )]
    #[OA\Response(
        response: 302,
        description: 'Authenticates the user and redirects to the frontend application'
    )]
    public function handleGoogleCallback(): RedirectResponse
    {
        try {
            $googleUser = Socialite::driver('google')->stateless()->user();

            $user = User::firstOrCreate(
                ['email' => $googleUser->getEmail()],
                [
                    'name' => $googleUser->getName(),
                    'password' => bcrypt(Str::random(16)),
                    'role' => 'guest',
                ]
            );

            Auth::login($user, true);

            $frontendUrl = env('FRONTEND_URL', 'http://localhost:5173');

            return redirect()->away($frontendUrl . '/');
        } catch (\Exception $e) {
            $frontendUrl = env('FRONTEND_URL', 'http://localhost:5173');

            return redirect()->away($frontendUrl . '/login?error=oauth_failed');
        }
    }
}
