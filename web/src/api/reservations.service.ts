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
    }
};