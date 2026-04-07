<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('audit_logs', function (Blueprint $blueprint) {
            $blueprint->id();
            $blueprint->foreignId('user_id')->constrained()->cascadeOnDelete();
            $blueprint->foreignId('restaurant_id')->constrained()->cascadeOnDelete();
            $blueprint->string('action');
            $blueprint->string('model_type')->nullable();
            $blueprint->unsignedBigInteger('model_id')->nullable();
            $blueprint->json('payload')->nullable();
            $blueprint->string('ip_address')->nullable();
            $blueprint->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('audit_logs');
    }
};
