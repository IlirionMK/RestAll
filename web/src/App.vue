<script setup lang="ts">
import { onMounted } from 'vue';
import { echo } from './echo';

onMounted(() => {
  echo.channel('test-channel')
      .listen('.test.event', (data: any) => {
        console.log('Real-time notification:', data.content);
      });
});
</script>

<template>
  <router-view />
</template>