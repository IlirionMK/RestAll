<?php

namespace App\Actions\Menu;

use App\Models\MenuCategory;

class UpdateMenuCategoryAction
{
    public function execute(MenuCategory $category, array $data): MenuCategory
    {
        $category->update($data);

        return $category;
    }
}
