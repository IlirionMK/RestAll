<?php

namespace App\Listeners;

use App\Events\UserActionPerformed;
use App\Models\AuditLog;
use Illuminate\Support\Facades\Request;

class RecordUserActivity
{
    public function handle(UserActionPerformed $event): void
    {
        AuditLog::create([
            'user_id' => auth()->id(),
            'restaurant_id' => auth()->user()->restaurant_id,
            'action' => $event->action,
            'model_type' => $event->model ? get_class($event->model) : null,
            'model_id' => $event->model?->id,
            'payload' => $event->payload,
            'ip_address' => Request::ip(),
        ]);
    }
}
