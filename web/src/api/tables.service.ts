import api from './axios';

export const TablesService = {
    getAll(params?: any) {
        return api.get('/api/tables', { params });
    },

    getById(id: number) {
        return api.get(`/api/tables/${id}`);
    },

    updateStatus(id: number, status: string) {
        return api.patch(`/api/tables/${id}/status`, { status });
    }
};