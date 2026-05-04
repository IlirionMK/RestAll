import api from './axios';

export const MenuService = {
    getCategories(restaurantId: number | string) {
        return api.get('/api/menu/categories', {
            params: { restaurant_id: restaurantId }
        });
    },

    getItems(restaurantId: number | string, categoryId?: number) {
        return api.get('/api/menu/items', {
            params: {
                restaurant_id: restaurantId,
                category_id: categoryId
            }
        });
    }
};