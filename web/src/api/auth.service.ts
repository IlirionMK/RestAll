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
    },

    async verify2FA(data: { code?: string; recovery_code?: string }): Promise<any> {
        const response = await api.post('/api/auth/2fa/challenge', data);
        return response.data;
    },

    async enable2FA(): Promise<any> {
        const response = await api.post('/api/auth/2fa/enable');
        return response.data;
    },

    async disable2FA(): Promise<any> {
        const response = await api.post('/api/auth/2fa/disable');
        return response.data;
    },

    async get2FAQR(): Promise<any> {
        const response = await api.get('/api/auth/2fa/qr-code');
        return response.data;
    },

    async get2FARecoveryCodes(): Promise<any> {
        const response = await api.get('/api/auth/2fa/recovery-codes');
        return response.data;
    },

    async confirm2FA(data: { code: string }): Promise<any> {
        const response = await api.post('/api/auth/2fa/confirm', data);
        return response.data;
    },

    async sendPasswordResetLink(data: { email: string }): Promise<any> {
        await this.getCsrfCookie();
        const response = await api.post('/api/auth/forgot-password', data);
        return response.data;
    },

    async resetPassword(data: Record<string, any>): Promise<any> {
        await this.getCsrfCookie();
        const response = await api.post('/api/auth/reset-password', data);
        return response.data;
    }
};