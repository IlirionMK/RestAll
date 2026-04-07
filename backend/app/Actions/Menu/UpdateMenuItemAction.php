<?php

namespace App\Actions\Menu;

use App\Models\MenuItem;

class UpdateMenuItemAction
{
    public function execute(MenuItem $item, array $data): MenuItem
    {
        $item->update($data);
        return $item;
    }
}
