import api from './axios';

export const RestaurantsService = {
    index(search?: string) {
        return api.get('/api/restaurants', {
            params: { search }
        });
    },
    show(id: number | string) {
        return api.get(`/api/restaurants/${id}`);
    }
};