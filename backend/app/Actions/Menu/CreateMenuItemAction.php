<?php

namespace App\Actions\Menu;

use App\Models\MenuItem;

class CreateMenuItemAction
{
    public function execute(array $data): MenuItem
    {
        return MenuItem::create($data);
    }
}
