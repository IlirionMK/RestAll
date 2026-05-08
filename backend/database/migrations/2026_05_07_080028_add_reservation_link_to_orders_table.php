<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('orders', function (Blueprint $blueprint) {
            $blueprint->foreignId('reservation_id')
                ->nullable()
                ->constrained('reservations')
                ->nullOnDelete();

            $blueprint->timestamp('paid_at')->nullable();
        });
    }

    public function down(): void
    {
        Schema::table('orders', function (Blueprint $blueprint) {
            $blueprint->dropConstrainedForeignId('reservation_id');
            $blueprint->dropColumn('paid_at');
        });
    }
};
