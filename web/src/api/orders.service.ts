import api from './axios';

export const OrdersService = {
    async index() {
        const { data } = await api.get('/api/orders');
        return data;
    },

    async store(orderData: any) {
        const { data } = await api.post('/api/orders', orderData);
        return data;
    },

    async show(orderId: number | string) {
        const { data } = await api.get(`/api/orders/${orderId}`);
        return data;
    },

    async addItems(orderId: number | string, items: any[]) {
        const { data } = await api.post(`/api/orders/${orderId}/items`, { items });
        return data;
    },

    async removeItem(orderItemId: number | string) {
        const { data } = await api.delete(`/api/orders/items/${orderItemId}`);
        return data;
    },

    async pay(orderId: number | string) {
        const { data } = await api.patch(`/api/orders/${orderId}/pay`);
        return data;
    }
};