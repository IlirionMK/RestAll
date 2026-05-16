<template>
  <Teleport to="body">
    <div class="fixed top-4 right-4 z-[200] flex flex-col gap-3 max-w-sm w-full pointer-events-none">
      <TransitionGroup
          enter-active-class="transition duration-300 ease-out"
          enter-from-class="opacity-0 translate-x-8"
          enter-to-class="opacity-100 translate-x-0"
          leave-active-class="transition duration-200 ease-in"
          leave-from-class="opacity-100 translate-x-0"
          leave-to-class="opacity-0 translate-x-8"
      >
        <div
            v-for="toast in toasts"
            :key="toast.id"
            :class="['pointer-events-auto flex items-start gap-3 p-4 rounded-2xl shadow-xl border cursor-pointer', typeClasses[toast.type]]"
            @click="dismiss(toast.id)"
        >
          <component :is="icons[toast.type]" class="w-5 h-5 flex-shrink-0 mt-0.5" />
          <p class="text-sm font-bold leading-snug flex-1">{{ toast.message }}</p>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { CheckCircle2, XCircle, AlertTriangle, Info } from 'lucide-vue-next';
import { useToast } from '@/composables/useToast';

const { toasts, dismiss } = useToast();

const typeClasses = {
  success: 'bg-white dark:bg-gray-900 border-restall-green/30 text-restall-green',
  error: 'bg-white dark:bg-gray-900 border-rose-200 dark:border-rose-500/30 text-rose-500',
  warning: 'bg-white dark:bg-gray-900 border-restall-gold/30 text-restall-gold',
  info: 'bg-white dark:bg-gray-900 border-gray-100 dark:border-gray-800 text-gray-600 dark:text-gray-300',
};

const icons = {
  success: CheckCircle2,
  error: XCircle,
  warning: AlertTriangle,
  info: Info,
};
</script>
