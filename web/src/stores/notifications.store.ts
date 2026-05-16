import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export type NotifType = 'billing_requested' | 'kitchen_ready' | 'items_added';

export interface Notification {
    id: string;
    type: NotifType;
    message: string;
    timestamp: Date;
    read: boolean;
    orderId?: number;
    tableNumber?: string | number;
}

export const useNotificationsStore = defineStore('notifications', () => {
    const items = ref<Notification[]>([]);
    const unreadCount = computed(() => items.value.filter(n => !n.read).length);

    function push(notif: Omit<Notification, 'id' | 'timestamp' | 'read'>) {
        items.value.unshift({
            ...notif,
            id: `${Date.now()}-${Math.random()}`,
            timestamp: new Date(),
            read: false,
        });
        if (items.value.length > 50) {
            items.value = items.value.slice(0, 50);
        }
    }

    function markAllRead() {
        items.value.forEach(n => { n.read = true; });
    }

    return { items, unreadCount, push, markAllRead };
});
