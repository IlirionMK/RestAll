<template>
  <div class="space-y-12">
    <section class="text-center py-12">
      <h1 class="text-5xl font-black mb-4">RestAll</h1>
      <p class="text-xl text-gray-500">{{ t('public.welcome_msg') }}</p>
    </section>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
      <div v-if="!authStore.activeBooking" class="p-8 bg-white dark:bg-gray-800 rounded-3xl border border-gray-100 dark:border-gray-700 shadow-xl">
        <CalendarCheck class="w-12 h-12 text-emerald-500 mb-4" />
        <h2 class="text-2xl font-bold mb-2">{{ t('public.book_table') }}</h2>
        <p class="text-gray-500 mb-6">{{ t('public.book_desc') }}</p>
        <router-link to="/bookings" class="inline-block px-8 py-4 bg-emerald-600 text-white font-bold rounded-2xl">
          {{ t('public.book_btn') }}
        </router-link>
      </div>

      <div v-else class="p-8 bg-emerald-50 dark:bg-emerald-500/10 rounded-3xl border border-emerald-100 dark:border-emerald-500/20">
        <Utensils class="w-12 h-12 text-emerald-500 mb-4" />
        <h2 class="text-2xl font-bold mb-2 text-emerald-900 dark:text-emerald-100">
          {{ authStore.canOrder ? t('public.ready_to_order') : t('public.waiting_confirmation') }}
        </h2>
        <p class="mb-6 text-emerald-700/70 dark:text-emerald-400">
          {{ authStore.canOrder ? t('public.table_msg', { id: authStore.activeBooking.table_id }) : t('public.please_wait') }}
        </p>
        <router-link
            v-if="authStore.canOrder"
            :to="`/order/${authStore.activeBooking.table_id}`"
            class="inline-block px-8 py-4 bg-emerald-600 text-white font-bold rounded-2xl shadow-lg"
        >
          {{ t('public.open_menu') }}
        </router-link>
      </div>

      <div class="p-8 bg-white dark:bg-gray-800 rounded-3xl border border-gray-100 dark:border-gray-700 shadow-xl">
        <Star class="w-12 h-12 text-amber-500 mb-4" />
        <h2 class="text-2xl font-bold mb-2">{{ t('public.reviews_title') }}</h2>
        <p class="text-gray-500 mb-6">{{ t('public.reviews_desc') }}</p>
        <router-link to="/reviews" class="inline-block px-8 py-4 bg-gray-900 text-white font-bold rounded-2xl">
          {{ t('public.read_reviews') }}
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { CalendarCheck, Utensils, Star } from 'lucide-vue-next';
import { useAuthStore } from '@/stores/auth.store';

const { t } = useI18n();
const authStore = useAuthStore();
</script>