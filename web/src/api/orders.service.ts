import api from './axios';

export const OrdersService = {
    getContext() {
        return api.get('/api/orders/context');
    },

    index() {
        return api.get('/api/orders');
    },

    store(data: any) {
        return api.post('/api/orders', data);
    },

    show(id: number | string) {
        return api.get(`/api/orders/${id}`);
    },

    addItems(id: number | string, items: any[]) {
        return api.post(`/api/orders/${id}/items`, { items });
    },

    removeItem(orderItemId: number) {
        return api.delete(`/api/orders/items/${orderItemId}`);
    },

    pay(id: number | string) {
        return api.patch(`/api/orders/${id}/pay`);
    }
};