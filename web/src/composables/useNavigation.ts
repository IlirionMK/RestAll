import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth.store';
import {
    LayoutDashboard,
    Utensils,
    Calendar,
    Star,
    Instagram,
    Facebook,
    Twitter
} from 'lucide-vue-next';

export function useNavigation() {
    const { t, locale } = useI18n();
    const authStore = useAuthStore();
    const isMobileMenuOpen = ref(false);

    const toggleMobileMenu = () => {
        isMobileMenuOpen.value = !isMobileMenuOpen.value;
    };

    const closeMobileMenu = () => {
        isMobileMenuOpen.value = false;
    };

    const publicLinks = [
        { name: 'Home', path: '/', label: 'nav.home' },
        { name: 'Menu', path: '/menu', label: 'nav.menu' },
        { name: 'Reviews', path: '/reviews', label: 'nav.reviews' },
    ];

    const footerSections = [
        {
            title: 'footer.explore',
            links: [
                { label: 'nav.home', path: '/' },
                { label: 'nav.menu', path: '/menu' },
                { label: 'nav.bookings', path: '/bookings' },
            ]
        },
        {
            title: 'footer.support',
            links: [
                { label: 'nav.contact', path: '/contact' },
                { label: 'nav.privacy', path: '/privacy' },
                { label: 'nav.terms', path: '/terms' }
            ]
        }
    ];

    const socialLinks = [
        { icon: Instagram, href: '#' },
        { icon: Facebook, href: '#' },
        { icon: Twitter, href: '#' }
    ];

    const userActions = computed(() => {
        const role = authStore.userRole;
        const actions = [
            { label: 'public.my_bookings', path: '/bookings', icon: Calendar }
        ];

        if (role === 'admin') {
            actions.unshift({ label: 'public.dashboard', path: '/admin', icon: LayoutDashboard });
        } else if (role === 'chef') {
            actions.unshift({ label: 'public.dashboard', path: '/kitchen', icon: LayoutDashboard });
        }

        return actions;
    });

    return {
        isMobileMenuOpen,
        toggleMobileMenu,
        closeMobileMenu,
        publicLinks,
        footerSections,
        socialLinks,
        userActions,
        t,
        locale
    };
}