<?php

namespace App\Enums;

enum ReservationStatus: string
{
    case PENDING = 'pending';
    case CONFIRMED = 'confirmed';
    case COMPLETED = 'completed';
    case CANCELLED = 'cancelled';
    case NO_SHOW = 'no_show';

    public static function getBusyStatuses(): array
    {
        return [
            self::PENDING->value,
            self::CONFIRMED->value,
        ];
    }
}
