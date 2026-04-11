import { createRouter, createWebHistory } from 'vue-router';
import type { RouteRecordRaw } from 'vue-router';
import { useAuthStore } from '../stores/auth.store';

let isInitialized = false;

const routes: RouteRecordRaw[] = [
    {
        path: '/',
        component: () => import('../layouts/AuthLayout.vue'),
        children: [
            { path: '', name: 'Home', component: () => import('../pages/public/Home.vue') },
            { path: 'login', name: 'Login', component: () => import('../pages/auth/Login.vue'), meta: { guestOnly: true } },
            { path: 'register', name: 'Register', component: () => import('../pages/auth/Register.vue'), meta: { guestOnly: true } },
            { path: 'bookings', name: 'UserBookings', component: () => import('../pages/public/Bookings.vue'), meta: { requiresAuth: true } },
            { path: 'menu', name: 'PublicMenu', component: () => import('../pages/public/Order.vue') },
            { path: 'order/:tableId', name: 'Order', component: () => import('../pages/public/Order.vue'), meta: { requiresAuth: true } },
            { path: 'reviews', name: 'Reviews', component: () => import('../pages/public/Reviews.vue') },
            { path: 'contact', name: 'Contact', component: () => import('../pages/public/Home.vue') },
            { path: 'privacy', name: 'Privacy', component: () => import('../pages/public/Home.vue') },
            { path: 'terms', name: 'Terms', component: () => import('../pages/public/Home.vue') }
        ]
    },
    {
        path: '/admin',
        component: () => import('../layouts/AdminLayout.vue'),
        meta: { requiresAuth: true, role: 'admin' },
        children: [
            { path: '', name: 'AdminDashboard', component: () => import('../pages/admin/AdminDashboard.vue') },
            { path: 'menu', name: 'AdminMenu', component: () => import('../pages/admin/AdminMenu.vue') },
            { path: 'staff', name: 'AdminStaff', component: () => import('../pages/admin/AdminStaff.vue') },
            { path: 'logs', name: 'AdminLogs', component: () => import('../pages/admin/AdminLogs.vue') }
        ]
    },
    {
        path: '/kitchen',
        component: () => import('../layouts/KitchenLayout.vue'),
        meta: { requiresAuth: true, role: 'chef' },
        children: [
            { path: '', name: 'KitchenDashboard', component: () => import('../pages/kitchen/KitchenDashboard.vue') }
        ]
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

router.beforeEach(async (to) => {
    const authStore = useAuthStore();

    if (!isInitialized) {
        await authStore.fetchUser();
        isInitialized = true;
    }

    const isAuthenticated = authStore.isAuth;
    const userRole = authStore.userRole;

    if (to.meta.requiresAuth && !isAuthenticated) {
        return { name: 'Login' };
    }

    if (to.meta.guestOnly && isAuthenticated) {
        return { name: 'Home' };
    }

    if (to.meta.role && to.meta.role !== userRole) {
        return { name: 'Home' };
    }
});

export default router;