<template>
  <component
      :is="to ? 'router-link' : 'button'"
      :to="to"
      :class="[
      'inline-flex items-center justify-center font-bold transition-all active:scale-95 disabled:opacity-50',
      variants[variant],
      sizes[size]
    ]"
  >
    <slot name="icon-left" />
    <slot />
    <slot name="icon-right" />
  </component>
</template>

<script setup lang="ts">
interface Props {
  to?: string;
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
}

withDefaults(defineProps<Props>(), {
  variant: 'primary',
  size: 'md'
});

const variants = {
  primary: 'bg-emerald-600 text-white hover:bg-emerald-700 shadow-lg shadow-emerald-600/20 rounded-xl',
  secondary: 'bg-gray-900 text-white dark:bg-white dark:text-gray-900 rounded-xl',
  outline: 'border-2 border-gray-200 dark:border-gray-700 hover:border-emerald-500 rounded-xl',
  ghost: 'hover:bg-gray-100 dark:hover:bg-gray-800 rounded-xl',
  danger: 'text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10 rounded-xl'
};

const sizes = {
  sm: 'px-3 py-1.5 text-xs',
  md: 'px-5 py-2.5 text-sm',
  lg: 'px-8 py-4 text-base'
};
</script>