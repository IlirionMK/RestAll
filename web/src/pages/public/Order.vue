<template>
  <div class="max-w-7xl mx-auto px-6 py-12 w-full font-sans">
    <div class="flex flex-col md:flex-row md:items-end justify-between gap-6 mb-12">
      <div>
        <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">{{ t('nav.menu') }}</h1>
        <p class="text-restall-green font-black uppercase tracking-widest text-xs mt-2">
          {{ t('order.table') }} #{{ $route.params.tableId || '—' }}
        </p>
      </div>

      <div class="flex bg-gray-100 dark:bg-gray-800 p-1.5 rounded-2xl border border-transparent dark:border-gray-700 shadow-sm transition-colors duration-300">
        <button v-for="type in ['dine_in', 'take_away']" :key="type"
                @click="orderType = type"
                :class="[
            'px-6 py-3 text-sm font-black rounded-xl transition-all duration-300',
            orderType === type ? 'bg-white dark:bg-gray-700 shadow-sm text-restall-green' : 'text-gray-500 hover:text-restall-dark dark:text-gray-400 dark:hover:text-restall-light'
          ]"
        >
          {{ t(`order.type.${type}`) }}
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-10">
      <div class="lg:col-span-2 space-y-6">
        <div class="p-16 border-2 border-dashed border-gray-200 dark:border-gray-700 rounded-squircle text-center text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-800/50 font-medium transition-colors duration-300">
          {{ t('order.loading_menu') }}
        </div>
      </div>

      <aside class="relative">
        <div class="bg-white dark:bg-gray-800 p-8 rounded-squircle shadow-2xl shadow-restall-green/5 border border-gray-100 dark:border-gray-700 sticky top-28 transition-colors duration-300">
          <h2 class="text-2xl font-black mb-6 text-restall-dark dark:text-restall-light">{{ t('order.cart') }}</h2>
          <div class="py-12 text-center text-gray-500 dark:text-gray-400 text-sm font-medium">
            {{ t('order.cart_empty') }}
          </div>
          <BaseButton variant="primary" class="w-full py-4 text-lg" disabled>
            {{ t('order.checkout') }}
          </BaseButton>
        </div>
      </aside>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const orderType = ref('dine_in');
</script>