import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth.store';
import { UserRole } from '@/types/auth.types';
import {
    LayoutDashboard,
    Calendar,
    Search,
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
        { name: 'Home', label: 'nav.home' },
        { name: 'BookingForm', label: 'nav.restaurants' },
        { name: 'PublicMenu', label: 'nav.menu' },
        { name: 'Reviews', label: 'nav.reviews' },
    ];

    const footerSections = [
        {
            title: 'footer.explore',
            links: [
                { label: 'nav.home', name: 'Home' },
                { label: 'nav.restaurants', name: 'BookingForm' },
                { label: 'nav.menu', name: 'PublicMenu' },
                { label: 'nav.bookings', name: 'UserBookings' },
            ]
        },
        {
            title: 'footer.support',
            links: [
                { label: 'nav.contact', name: 'Contact' },
                { label: 'nav.privacy', name: 'Privacy' },
                { label: 'nav.terms', name: 'Terms' }
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
        const actions: any[] = [];

        if (!role || role === UserRole.GUEST) {
            actions.push({
                label: 'public.my_bookings',
                name: 'UserBookings',
                icon: Calendar
            });
        }

        if (role === UserRole.ADMIN) {
            actions.unshift({ label: 'public.dashboard', name: 'AdminDashboard', icon: LayoutDashboard });
        } else if (role === UserRole.CHEF) {
            actions.unshift({ label: 'public.dashboard', name: 'KitchenDashboard', icon: LayoutDashboard });
        } else if (role === UserRole.WAITER) {
            actions.unshift({ label: 'public.dashboard', name: 'WaiterDashboard', icon: LayoutDashboard });
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