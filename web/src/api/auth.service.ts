import api from './axios';

export const AuthService = {
    async getCsrfCookie(): Promise<void> {
        return api.get('/sanctum/csrf-cookie');
    },

    async login(credentials: any): Promise<any> {
        await this.getCsrfCookie();
        return api.post('/api/auth/login', credentials);
    },

    async register(data: any): Promise<any> {
        await this.getCsrfCookie();
        return api.post('/api/auth/register', data);
    },

    async logout(): Promise<void> {
        return api.post('/api/auth/logout');
    },

    async getCurrentUser(): Promise<any> {
        const { data } = await api.get('/api/users/me');
        return data;
    }
};