<?php

declare(strict_types=1);

namespace App\Listeners;

use App\Events\UserActionPerformed;
use App\Models\AuditLog;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Request;

class RecordUserActivity
{
    public function handle(UserActionPerformed $event): void
    {
        $user = Auth::user();

        AuditLog::create([
            'user_id' => $user?->id ?? $event->model->user_id ?? null,
            'restaurant_id' => $user?->restaurant_id ?? $event->model->restaurant_id ?? null,
            'action' => $event->action,
            'model_type' => $event->model ? get_class($event->model) : null,
            'model_id' => $event->model?->id,
            'payload' => $event->payload,
            'ip_address' => Request::ip() ?? '127.0.0.1',
        ]);
    }
}
