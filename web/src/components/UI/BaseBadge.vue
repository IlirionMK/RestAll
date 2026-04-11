<template>
  <span :class="[
    'inline-flex items-center px-3 py-1 text-[10px] font-black uppercase tracking-widest rounded-xl',
    variantClasses[variant]
  ]">
    <slot />
  </span>
</template>

<script setup lang="ts">
const props = defineProps({
  variant: {
    type: String,
    default: 'default'
  }
});

const variantClasses: Record<string, string> = {
  default: 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-300',
  green: 'bg-restall-green text-restall-light',
  gold: 'bg-restall-gold text-restall-light',
  dark: 'bg-restall-dark text-restall-light dark:bg-restall-light dark:text-restall-dark'
};
</script>