<?php

namespace App\Actions\Table;

use App\Models\Table;

class UpdateTableStatusAction
{
    public function execute(Table $table, string $status): Table
    {
        $table->update(['status' => $status]);

        return $table;
    }
}
