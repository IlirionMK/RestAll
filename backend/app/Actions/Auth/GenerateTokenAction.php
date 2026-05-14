<?php

declare(strict_types=1);

namespace App\Actions\Auth;

use App\Models\User;

class GenerateTokenAction
{
    public function execute(User $user, string $deviceName): string
    {
        $user->tokens()->where('name', $deviceName)->delete();

        return $user->createToken($deviceName)->plainTextToken;
    }
}
