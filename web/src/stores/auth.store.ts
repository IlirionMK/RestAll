import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { API } from '@/api';

export const useAuthStore = defineStore('auth', () => {
    const user = ref<any>(null);
    const activeBooking = ref<{ id: number; table_id: number; status: string } | null>(null);

    const isAuth = computed(() => !!user.value);
    const userRole = computed(() => user.value?.role || null);
    const canOrder = computed(() => activeBooking.value?.status === 'confirmed');

    const fetchUser = async () => {
        try {
            const data = await API.auth.getCurrentUser();
            user.value = data;
            activeBooking.value = data?.active_booking || null;
        } catch (error) {
            user.value = null;
            activeBooking.value = null;
        }
    };

    const login = async (credentials: any) => {
        try {
            await API.auth.login(credentials);
            await fetchUser();
        } catch (error) {
            user.value = null;
            throw error;
        }
    };

    const register = async (userData: any) => {
        try {
            await API.auth.register(userData);
        } catch (error) {
            throw error;
        }
    };

    const logout = async () => {
        try {
            await API.auth.logout();
        } finally {
            user.value = null;
            activeBooking.value = null;
        }
    };

    return {
        user,
        activeBooking,
        isAuth,
        userRole,
        canOrder,
        fetchUser,
        login,
        register,
        logout
    };
});