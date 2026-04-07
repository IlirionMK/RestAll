<?php

namespace App\Actions\Menu;

use App\Models\MenuItem;

class ToggleMenuItemAvailabilityAction
{
    public function execute(MenuItem $item): MenuItem
    {
        $item->update(['is_available' => !$item->is_available]);
        return $item;
    }
}
