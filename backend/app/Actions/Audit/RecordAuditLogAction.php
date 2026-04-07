<?php

namespace App\Actions\Audit;

use App\Models\AuditLog;
use Illuminate\Http\Request;
use Illuminate\Database\Eloquent\Model;

class RecordAuditLogAction
{
    public function execute(Request $request, string $action, ?Model $model = null): void
    {
        AuditLog::create([
            'user_id' => $request->user()->id,
            'restaurant_id' => $request->user()->restaurant_id,
            'action' => $action,
            'model_type' => $model ? get_class($model) : null,
            'model_id' => $model ? $model->id : null,
            'payload' => $request->except(['password', 'password_confirmation']),
            'ip_address' => $request->ip(),
        ]);
    }
}
