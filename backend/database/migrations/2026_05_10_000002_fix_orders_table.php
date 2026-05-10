<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        DB::table('orders')->where('status', 'open')->update(['status' => 'pending']);

        Schema::table('orders', function (Blueprint $table) {
            $table->string('status')->default('pending')->change();
            $table->boolean('is_takeaway')->default(false)->after('status');
        });

        if (DB::getDriverName() === 'pgsql') {
            DB::statement("ALTER TABLE orders ADD CONSTRAINT orders_status_check CHECK (status IN ('pending', 'billing_requested', 'paid', 'cancelled'))");
        }
    }

    public function down(): void
    {
        if (DB::getDriverName() === 'pgsql') {
            DB::statement("ALTER TABLE orders DROP CONSTRAINT IF EXISTS orders_status_check");
        }

        Schema::table('orders', function (Blueprint $table) {
            $table->dropColumn('is_takeaway');
            $table->string('status')->default('open')->change();
        });
    }
};
