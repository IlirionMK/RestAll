import api from './axios';

export interface MenuItemData {
    menu_category_id: number;
    name: string;
    description?: string;
    price: number;
    is_available?: boolean;
}

export const MenuService = {
    getCategories(restaurantId?: number | string) {
        return api.get('/api/menu/categories', {
            params: restaurantId ? { restaurant_id: restaurantId } : undefined
        });
    },

    getItems(restaurantId: number | string, categoryId?: number) {
        return api.get('/api/menu/items', {
            params: {
                restaurant_id: restaurantId,
                category_id: categoryId
            }
        });
    },

    storeItem(data: MenuItemData) {
        return api.post('/api/menu/items', data);
    },

    updateItem(id: number | string, data: Partial<MenuItemData>) {
        return api.put(`/api/menu/items/${id}`, data);
    },

    toggleAvailability(id: number | string) {
        return api.patch(`/api/menu/items/${id}/availability`);
    },

    destroyItem(id: number | string) {
        return api.delete(`/api/menu/items/${id}`);
    },
};