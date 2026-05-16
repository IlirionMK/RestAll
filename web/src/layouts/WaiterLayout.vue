<template>
  <div class="min-h-screen bg-[#FAF9F6] font-sans antialiased">
    <StaffHeader
        brand-label="Waiter"
        variant="light"
        :nav-items="navItems"
    />
    <main class="max-w-5xl mx-auto px-4 py-8">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted } from 'vue';
import { useI18n } from 'vue-i18n';
import StaffHeader from '@/components/staff/StaffHeader.vue';
import { useAuthStore } from '@/stores/auth.store';
import { useNotificationsStore } from '@/stores/notifications.store';
import { echo } from '@/echo';

const { t } = useI18n();
const authStore = useAuthStore();
const notifStore = useNotificationsStore();

const navItems = computed(() => [
  { label: t('waiter.dashboard'), to: '/waiter' },
  { label: t('waiter.orders'),    to: '/waiter/orders' },
]);

const handleBillingNotif = (data: any) => {
  const tableNum = data.table_number ?? data.table?.number;
  const msg = tableNum
    ? `${t('notif.table')} ${tableNum} — ${t('notif.billing_requested')}`
    : `${t('notif.takeaway')} — ${t('notif.billing_requested')}`;
  notifStore.push({ type: 'billing_requested', message: msg, orderId: data.order_id ?? data.id, tableNumber: tableNum });
};

const handleKitchenReady = (data: any) => {
  if (data.status !== 'ready') return;
  const tableNum = data.table_number;
  const itemName = data.name ?? '';
  const msg = tableNum
    ? `${t('notif.table')} ${tableNum} — ${itemName} ${t('notif.kitchen_ready')}`
    : `${t('notif.takeaway')} — ${itemName} ${t('notif.kitchen_ready')}`;
  notifStore.push({ type: 'kitchen_ready', message: msg, orderId: data.order_id, tableNumber: tableNum });
};

let staffChannel: any = null;
let kitchenChannel: any = null;

onMounted(() => {
  const restaurantId = authStore.user?.restaurant_id;
  if (!restaurantId) return;

  staffChannel = echo.private(`restaurant.${restaurantId}.staff`);
  staffChannel.listen('.order.billing_requested', handleBillingNotif);

  kitchenChannel = echo.private(`restaurant.${restaurantId}.kitchen`);
  kitchenChannel.listen('.kitchen.ticket_status_updated', handleKitchenReady);
});

onUnmounted(() => {
  staffChannel?.stopListening('.order.billing_requested', handleBillingNotif);
  kitchenChannel?.stopListening('.kitchen.ticket_status_updated', handleKitchenReady);
});
</script>
