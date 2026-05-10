import api from './axios';

export const ReservationsService = {
    async index() {
        const { data } = await api.get('/api/reservations');
        return data;
    },

    async create(bookingData: any) {
        const { data } = await api.post('/api/reservations', bookingData);
        return data;
    },

    async destroy(reservationId: number | string) {
        const { data } = await api.delete(`/api/reservations/${reservationId}`);
        return data;
    },

    async createOrder(reservationId: number | string, isTakeaway: boolean = false) {
        const { data } = await api.post(`/api/reservations/${reservationId}/orders`, { is_takeaway: isTakeaway });
        return data;
    },

    async getOrder(reservationId: number | string) {
        const { data } = await api.get(`/api/reservations/${reservationId}/order`);
        return data;
    }
};