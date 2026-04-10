import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost',
    withCredentials: true,
    headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
    },
});

export const getCsrfCookie = () => api.get('/sanctum/csrf-cookie');

api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            if (error.config.url?.includes('/api/users/me')) {
                return Promise.resolve({ data: null });
            }
        }
        return Promise.reject(error);
    }
);

export default api;