import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { API } from '@/api';

export const useAuthStore = defineStore('auth', () => {
    const user = ref<any>(null);
    const requires2FA = ref(false);
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

    const loginWithGoogle = () => {
        window.location.href = `${import.meta.env.VITE_API_BASE_URL}/api/auth/google/redirect`;
    };

    const login = async (credentials: any) => {
        try {
            const data = await API.auth.login(credentials);

            if (data?.two_factor) {
                requires2FA.value = true;
                return;
            }

            requires2FA.value = false;
            await fetchUser();
        } catch (error) {
            user.value = null;
            requires2FA.value = false;
            throw error;
        }
    };

    const verify2FA = async (code: string) => {
        try {
            await API.auth.verify2FA({ code });
            requires2FA.value = false;
            await fetchUser();
        } catch (error) {
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
            requires2FA.value = false;
        }
    };

    return {
        user,
        requires2FA,
        activeBooking,
        isAuth,
        userRole,
        canOrder,
        fetchUser,
        login,
        loginWithGoogle,
        verify2FA,
        register,
        logout
    };
});