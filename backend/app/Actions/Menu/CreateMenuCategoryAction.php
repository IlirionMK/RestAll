<?php

namespace App\Actions\Menu;

use App\Models\MenuCategory;

class CreateMenuCategoryAction
{
    public function execute(array $data): MenuCategory
    {
        return MenuCategory::create($data);
    }
}
