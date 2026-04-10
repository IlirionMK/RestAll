import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api, { getCsrfCookie } from '../api/axios';

export const useAuthStore = defineStore('auth', () => {
    const user = ref<any>(null);
    const activeBooking = ref<{ id: number; table_id: number; status: string } | null>(null);

    const isAuth = computed(() => !!user.value);
    const userRole = computed(() => user.value?.role || null);
    const canOrder = computed(() => activeBooking.value?.status === 'confirmed');

    const fetchUser = async () => {
        try {
            const { data } = await api.get('/api/users/me');
            user.value = data.user;
            activeBooking.value = data.active_booking;
        } catch {
            user.value = null;
            activeBooking.value = null;
        }
    };

    const login = async (credentials: any) => {
        await getCsrfCookie();
        await api.post('/api/auth/login', credentials);
        await fetchUser();
    };

    const logout = async () => {
        try { await api.post('/api/auth/logout'); }
        finally { user.value = null; activeBooking.value = null; }
    };

    return { user, activeBooking, isAuth, userRole, canOrder, fetchUser, login, logout };
});