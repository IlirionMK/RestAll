<?php

declare(strict_types=1);

namespace App\Enums;

enum OrderStatus: string
{
    case PENDING = 'pending';
    case BILLING_REQUESTED = 'billing_requested';
    case PAID = 'paid';
    case CANCELLED = 'cancelled';
}