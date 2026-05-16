import axios from 'axios';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'X-Requested-With': 'XMLHttpRequest',
    },
    withCredentials: true,
    withXSRFToken: true
});

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 419 && !originalRequest._retry) {
            originalRequest._retry = true;

            try {
                await axios.get(`${import.meta.env.VITE_API_BASE_URL}/sanctum/csrf-cookie`, {
                    withCredentials: true
                });

                return api(originalRequest);
            } catch (retryError) {
                return Promise.reject(retryError);
            }
        }

        if (error.response?.status === 401) {
            const isLogout = originalRequest.url?.includes('/auth/logout');
            if (!isLogout && !window.location.pathname.includes('/login')) {
                window.location.href = '/login';
            }
        }

        return Promise.reject(error);
    }
);

export default api;