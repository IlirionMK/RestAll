<?php

namespace App\Actions\Auth;

use Illuminate\Http\Request;

class RefreshTokenAction
{
    public function execute(Request $request): string
    {
        $request->user()->currentAccessToken()->delete();

        return $request->user()->createToken('auth_token')->plainTextToken;
    }
}
