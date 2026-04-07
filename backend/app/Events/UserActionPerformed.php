<?php

namespace App\Events;

use Illuminate\Foundation\Events\Dispatchable;
use Illuminate\Queue\SerializesModels;
use Illuminate\Database\Eloquent\Model;

class UserActionPerformed
{
    use Dispatchable, SerializesModels;

    public function __construct(
        public string $action,
        public ?Model $model = null,
        public array $payload = []
    ) {}
}
