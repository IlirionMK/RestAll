<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Support\Facades\DB;

return new class extends Migration
{
    public function up(): void
    {
        if (DB::getDriverName() !== 'pgsql') {
            return;
        }

        DB::statement('ALTER TABLE reservations DROP CONSTRAINT IF EXISTS reservations_status_check');
        DB::statement("ALTER TABLE reservations ADD CONSTRAINT reservations_status_check CHECK (status IN ('pending', 'confirmed', 'cancelled', 'completed', 'no_show'))");
    }

    public function down(): void
    {
        if (DB::getDriverName() !== 'pgsql') {
            return;
        }

        DB::statement('ALTER TABLE reservations DROP CONSTRAINT IF EXISTS reservations_status_check');
        DB::statement("ALTER TABLE reservations ADD CONSTRAINT reservations_status_check CHECK (status IN ('pending', 'confirmed', 'cancelled', 'completed'))");
    }
};
