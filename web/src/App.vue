<script setup lang="ts">
import { watch } from 'vue';
import { echo } from './echo';
import { useAuthStore } from '@/stores/auth.store';
import { useToast } from '@/composables/useToast';
import ToastNotification from '@/components/UI/ToastNotification.vue';

const auth = useAuthStore();
const { warning } = useToast();

watch(() => auth.user, (user, prevUser) => {
  if (prevUser?.restaurant_id) {
    echo.leave(`restaurant.${prevUser.restaurant_id}.staff`);
  }

  if (user?.restaurant_id) {
    echo.private(`restaurant.${user.restaurant_id}.staff`)
        .listen('.order.billing_requested', (data: any) => {
          warning(`Table ${data.table_number} requested the bill!`);
        });
  }
}, { immediate: true });
</script>

<template>
  <router-view />
  <ToastNotification />
</template>