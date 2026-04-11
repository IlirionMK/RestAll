import api from './axios';

export const KitchenService = {
    async getTickets() {
        const { data } = await api.get('/api/kitchen/tickets');
        return data;
    },

    async updateStatus(orderItemId: number | string, status: string) {
        const { data } = await api.patch(`/api/kitchen/tickets/${orderItemId}/status`, { status });
        return data;
    }
};