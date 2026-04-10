<template>
  <span :class="['px-2.5 py-1 rounded-full text-xs font-medium', variantClasses]">
    {{ text }}
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  text: string;
  variant?: 'success' | 'warning' | 'info' | 'danger';
}>();

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'success':
      return 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400';
    case 'warning':
      return 'bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400';
    case 'danger':
      return 'bg-rose-100 dark:bg-rose-900/30 text-rose-700 dark:text-rose-400';
    case 'info':
    default:
      return 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400';
  }
});
</script>