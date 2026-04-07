<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('users', function (Blueprint $table) {
            $table->foreignId('restaurant_id')->nullable()->constrained('restaurants')->cascadeOnDelete();
        });

        Schema::table('menu_categories', function (Blueprint $table) {
            $table->foreignId('restaurant_id')->nullable()->constrained('restaurants')->cascadeOnDelete();
        });

        Schema::table('menu_items', function (Blueprint $table) {
            $table->foreignId('restaurant_id')->nullable()->constrained('restaurants')->cascadeOnDelete();
        });
    }

    public function down(): void
    {
        Schema::table('users', function (Blueprint $table) {
            $table->dropForeign(['restaurant_id']);
            $table->dropColumn('restaurant_id');
        });

        Schema::table('menu_categories', function (Blueprint $table) {
            $table->dropForeign(['restaurant_id']);
            $table->dropColumn('restaurant_id');
        });

        Schema::table('menu_items', function (Blueprint $table) {
            $table->dropForeign(['restaurant_id']);
            $table->dropColumn('restaurant_id');
        });
    }
};
