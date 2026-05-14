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
            'email' => $user->email,
            'password' => 'password',
        ])->assertOk();
    }

    public function test_login_fails_with_wrong_password(): void
    {
        $user = User::factory()->create();

        $this->postJson('/api/auth/login', [
            'email' => $user->email,
            'password' => 'wrong-password',
        ])->assertUnprocessable();
    }

    public function test_login_fails_with_unknown_email(): void
    {
        $this->postJson('/api/auth/login', [
            'email' => 'nobody@example.com',
            'password' => 'password',
        ])->assertUnprocessable();
    }

    public function test_authenticated_user_can_refresh_token(): void
    {
        $user = User::factory()->create();
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

    public function test_user_can_obtain_token_with_valid_credentials(): void
    {
        $user = User::factory()->create(['password' => Hash::make('password')]);

        $this->postJson('/api/auth/token', [
            'email' => $user->email,
            'password' => 'password',
        ])
            ->assertOk()
            ->assertJsonStructure([
                'token',
                'user' => ['id', 'name', 'email', 'role', 'restaurant_id'],
            ]);
    }

    public function test_token_login_returns_correct_user_data(): void
    {
        $user = User::factory()->create(['password' => Hash::make('password')]);

        $response = $this->postJson('/api/auth/token', [
            'email' => $user->email,
            'password' => 'password',
        ])->assertOk();

        $this->assertEquals($user->id, $response->json('user.id'));
        $this->assertEquals($user->email, $response->json('user.email'));
    }

    public function test_token_login_fails_with_wrong_password(): void
    {
        $user = User::factory()->create();

        $this->postJson('/api/auth/token', [
            'email' => $user->email,
            'password' => 'wrong-password',
        ])->assertUnprocessable();
    }

    public function test_token_login_fails_with_unknown_email(): void
    {
        $this->postJson('/api/auth/token', [
            'email' => 'nobody@example.com',
            'password' => 'password',
        ])->assertUnprocessable();
    }

    public function test_repeated_token_login_replaces_existing_token_for_same_device(): void
    {
        $user = User::factory()->create(['password' => Hash::make('password')]);

        $this->postJson('/api/auth/token', [
            'email' => $user->email,
            'password' => 'password',
            'device_name' => 'restall.desktop',
        ])->assertOk();

        $this->postJson('/api/auth/token', [
            'email' => $user->email,
            'password' => 'password',
            'device_name' => 'restall.desktop',
        ])->assertOk();

        $this->assertEquals(1, $user->tokens()->where('name', 'restall.desktop')->count());
    }

    public function test_authenticated_user_can_revoke_token(): void
    {
        $user = User::factory()->create();
        $token = $user->createToken('restall.desktop')->plainTextToken;

        $this->withToken($token)
            ->deleteJson('/api/auth/token')
            ->assertOk()
            ->assertJsonFragment(['message' => 'Token revoked.']);

        $this->assertEquals(0, $user->tokens()->count());
    }

    public function test_unauthenticated_cannot_revoke_token(): void
    {
        $this->deleteJson('/api/auth/token')->assertUnauthorized();
    }
}
