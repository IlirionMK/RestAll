<?php

declare(strict_types=1);

namespace Tests\Feature;

use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Tests\TestCase;

class AuthApiTest extends TestCase
{
    use RefreshDatabase;

    public function test_user_can_login_with_valid_credentials(): void
    {
        $user = User::factory()->create(['password' => Hash::make('password')]);

        $this->postJson('/api/auth/login', [
            'email'    => $user->email,
            'password' => 'password',
        ])->assertOk();
    }

    public function test_login_fails_with_wrong_password(): void
    {
        $user = User::factory()->create();

        $this->postJson('/api/auth/login', [
            'email'    => $user->email,
            'password' => 'wrong-password',
        ])->assertUnprocessable();
    }

    public function test_login_fails_with_unknown_email(): void
    {
        $this->postJson('/api/auth/login', [
            'email'    => 'nobody@example.com',
            'password' => 'password',
        ])->assertUnprocessable();
    }

    public function test_authenticated_user_can_refresh_token(): void
    {
        $user  = User::factory()->create();
        $token = $user->createToken('test')->plainTextToken;

        $this->withToken($token)
            ->postJson('/api/auth/refresh')
            ->assertOk()
            ->assertJsonStructure(['token']);
    }

    public function test_unauthenticated_cannot_refresh_token(): void
    {
        $this->postJson('/api/auth/refresh')->assertUnauthorized();
    }
}
