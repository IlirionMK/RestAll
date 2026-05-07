<script setup lang="ts">
import { watch } from 'vue';
import { echo } from './echo';
import { useAuthStore } from '@/stores/auth.store';

const auth = useAuthStore();

echo.channel('test-channel')
    .listen('.test.event', (data: any) => {
      console.log('Real-time notification:', data.content);
    });

watch(() => auth.user, (user) => {
  if (user?.restaurant_id) {
    echo.private(`restaurant.${user.restaurant_id}.staff`)
        .listen('.order.billing_requested', (data: any) => {
          console.log('!!! BILL REQUEST RECEIVED !!!', data);
          alert(`Table ${data.table_number} requested the bill!`);
        })
        .error((err: any) => {
          console.error('Subscription error details:', err);
        });
  }
}, { immediate: true });
</script>

<template>
  <router-view />
</template>