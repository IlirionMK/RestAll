<template>
  <div class="bg-white dark:bg-restall-dark/50 p-6 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-800 transition-colors duration-300">
    <div class="flex items-start justify-between">
      <div>
        <p class="text-xs font-black uppercase tracking-widest text-gray-400 mb-1">{{ title }}</p>
        <h3 class="text-3xl font-black text-restall-dark dark:text-restall-light">{{ value }}</h3>
      </div>
      <div class="w-12 h-12 rounded-2xl bg-restall-light dark:bg-gray-800 flex items-center justify-center text-restall-green">
        <slot name="icon" />
      </div>
    </div>
    <div v-if="trend" class="mt-4 flex items-center text-sm">
      <span :class="trend > 0 ? 'text-restall-green' : 'text-rose-500'" class="font-bold flex items-center">
        <component :is="trend > 0 ? 'ArrowUpRight' : 'ArrowDownRight'" class="w-4 h-4 mr-1" />
        {{ Math.abs(trend) }}%
      </span>
      <span class="ml-2 text-gray-500 dark:text-gray-400 text-xs">vs last week</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ArrowUpRight, ArrowDownRight } from 'lucide-vue-next';

defineProps({
  title: {
    type: String,
    required: true
  },
  value: {
    type: [String, Number],
    required: true
  },
  trend: {
    type: Number,
    default: null
  }
});
</script>