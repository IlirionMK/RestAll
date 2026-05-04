import api from './axios';

export const AuthService = {
    async getCsrfCookie(): Promise<void> {
        return api.get('/sanctum/csrf-cookie');
    },

    async login(credentials: any): Promise<any> {
        await this.getCsrfCookie();
        const response = await api.post('/api/auth/login', credentials);
        return response.data;
    },

    async register(payload: any): Promise<any> {
        await this.getCsrfCookie();
        const response = await api.post('/api/auth/register', payload);
        return response.data;
    },

    async logout(): Promise<void> {
        return api.post('/api/auth/logout');
    },

    async getCurrentUser(): Promise<any> {
        const response = await api.get('/api/users/me');
        return response.data;
    }
};