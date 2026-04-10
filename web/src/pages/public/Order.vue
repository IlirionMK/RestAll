<template>
  <div class="space-y-8">
    <div class="flex flex-col md:flex-row md:items-end justify-between gap-4">
      <div>
        <h1 class="text-3xl font-black text-gray-900 dark:text-white">{{ t('nav.menu') }}</h1>
        <p class="text-emerald-600 font-bold">{{ t('order.table') }} #{{ $route.params.tableId }}</p>
      </div>

      <div class="flex bg-gray-100 dark:bg-gray-800 p-1 rounded-xl">
        <button v-for="type in ['dine_in', 'take_away']" :key="type"
                @click="orderType = type"
                :class="[
            'px-6 py-2 text-sm font-black rounded-lg transition-all',
            orderType === type ? 'bg-white dark:bg-gray-700 shadow-sm text-emerald-600' : 'text-gray-500'
          ]"
        >
          {{ t(`order.type.${type}`) }}
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div class="lg:col-span-2 space-y-6">
        <div class="p-12 border-2 border-dashed border-gray-200 dark:border-gray-800 rounded-3xl text-center text-gray-400">
          {{ t('order.loading_menu') }}
        </div>
      </div>

      <aside class="space-y-6">
        <div class="bg-white dark:bg-gray-800 p-6 rounded-3xl shadow-xl border border-gray-100 dark:border-gray-700 sticky top-24">
          <h2 class="text-xl font-black mb-4">{{ t('order.cart') }}</h2>
          <div class="py-8 text-center text-gray-400 text-sm">
            {{ t('order.cart_empty') }}
          </div>
          <BaseButton variant="primary" class="w-full" disabled>{{ t('order.checkout') }}</BaseButton>
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