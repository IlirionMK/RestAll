<?php

declare(strict_types=1);

namespace App\Enums;

enum OrderItemStatus: string
{
    case PENDING = 'pending';
    case PREPARING = 'preparing';
    case READY = 'ready';
    case DELIVERED = 'delivered';
}
