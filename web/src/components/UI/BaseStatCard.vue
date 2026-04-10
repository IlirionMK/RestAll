<template>
  <div class="bg-white dark:bg-gray-800 p-6 rounded-xl border border-gray-100 dark:border-gray-700 shadow-sm transition-colors duration-200">
    <div class="flex items-center justify-between">
      <h3 class="text-gray-500 dark:text-gray-400 text-sm font-medium">{{ title }}</h3>
      <div class="text-gray-400 dark:text-gray-500">
        <slot name="icon"></slot>
      </div>
    </div>
    <p class="text-3xl font-bold text-gray-800 dark:text-white mt-2">{{ value }}</p>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string;
  value: string | number;
}>();
</script>