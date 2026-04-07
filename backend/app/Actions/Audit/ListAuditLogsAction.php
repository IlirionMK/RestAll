<?php

namespace App\Actions\Audit;

use App\Models\AuditLog;
use Illuminate\Database\Eloquent\Collection;

class ListAuditLogsAction
{
    public function execute(int $limit = 50): Collection
    {
        return AuditLog::with('user')
            ->latest()
            ->limit($limit)
            ->get();
    }
}
