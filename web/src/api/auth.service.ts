import api from './axios';

export const AuthService = {
    async getCsrfCookie() {
        return api.get('/sanctum/csrf-cookie');
    },

    async login(credentials: any) {
        await this.getCsrfCookie();
        return api.post('/api/auth/login', credentials);
    },

    async register(data: any) {
        await this.getCsrfCookie();
        return api.post('/api/auth/register', data);
    },

    async logout() {
        return api.post('/api/auth/logout');
    },

    async getCurrentUser() {
        const { data } = await api.get('/api/users/me');
        return data;
    }
};