<template>
  <component
      :is="to ? 'router-link' : 'button'"
      :to="to"
      :class="[
      'inline-flex items-center justify-center font-bold transition-all duration-300 outline-none disabled:opacity-50 disabled:cursor-not-allowed no-underline hover:no-underline',
      sizeClasses[size],
      variantClasses[variant],
      squircle ? 'rounded-squircle' : ''
    ]"
      v-bind="$attrs"
  >
    <slot />
  </component>
</template>

<script setup lang="ts">
interface Props {
  to?: string | object;
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  squircle?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  to: undefined,
  variant: 'primary',
  size: 'md',
  squircle: false
});

const sizeClasses: Record<string, string> = {
  sm: 'px-4 py-2 text-sm',
  md: 'px-6 py-3 text-base',
  lg: 'px-8 py-4 text-lg'
};

const variantClasses: Record<string, string> = {
  primary: 'bg-restall-gold hover:opacity-90 text-restall-light shadow-lg shadow-restall-gold/30',
  secondary: 'bg-restall-green hover:opacity-90 text-restall-light shadow-lg shadow-restall-green/30',
  outline: 'border-2 border-white text-white hover:bg-white hover:text-restall-dark',
  ghost: 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800/50',
  danger: 'bg-rose-600 hover:bg-rose-700 text-white shadow-lg shadow-rose-600/30',
  link: 'text-restall-gold hover:opacity-70 bg-transparent p-0',
};
</script>