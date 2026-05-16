<template>
  <div class="h-screen flex flex-col bg-gray-950 text-white font-sans antialiased overflow-hidden">
    <StaffHeader brand-label="KDS" variant="dark" />
    <main class="flex-1 overflow-x-auto p-4 sm:p-8">
      <router-view v-slot="{ Component }">
        <Transition enter-active-class="transition duration-200 ease-out" enter-from-class="opacity-0 scale-95"
                    enter-to-class="opacity-100 scale-100" leave-active-class="transition duration-150 ease-in"
                    leave-from-class="opacity-100 scale-100" leave-to-class="opacity-0 scale-95" mode="out-in">
          <component :is="Component" />
        </Transition>
      </router-view>
    </main>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import StaffHeader from '@/components/staff/StaffHeader.vue';
import { useAuthStore } from '@/stores/auth.store';
import { useNotificationsStore } from '@/stores/notifications.store';
import { useI18n } from 'vue-i18n';
import { echo } from '@/echo';

const { t } = useI18n();
const authStore = useAuthStore();
const notifStore = useNotificationsStore();

const handleItemsAdded = (data: any) => {
  const tableNum = data.table_number;
  const msg = tableNum
    ? `${t('notif.table')} ${tableNum} — ${t('notif.items_added')}`
    : `${t('notif.takeaway')} — ${t('notif.items_added')}`;
  notifStore.push({ type: 'items_added', message: msg, orderId: data.order_id, tableNumber: tableNum });
};

let kitchenChannel: any = null;

onMounted(() => {
  const restaurantId = authStore.user?.restaurant_id;
  if (!restaurantId) return;

  kitchenChannel = echo.private(`restaurant.${restaurantId}.kitchen`);
  kitchenChannel.listen('.kitchen.items_added', handleItemsAdded);
});

onUnmounted(() => {
  kitchenChannel?.stopListening('.kitchen.items_added', handleItemsAdded);
});
</script>
