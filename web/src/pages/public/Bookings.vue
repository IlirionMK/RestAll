<template>
  <div class="max-w-2xl mx-auto space-y-8">
    <div class="text-center">
      <h1 class="text-3xl font-black text-gray-900 dark:text-white">{{ t('nav.bookings') }}</h1>
      <p class="text-gray-500 mt-2">{{ t('bookings.subtitle') }}</p>
    </div>

    <form @submit.prevent="handleBooking" class="bg-white dark:bg-gray-800 p-8 rounded-3xl shadow-xl border border-gray-100 dark:border-gray-700 space-y-6">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label class="block text-sm font-bold mb-2">{{ t('bookings.date') }}</label>
          <input v-model="form.date" type="date" required class="w-full px-4 py-3 rounded-xl border border-gray-200 dark:border-gray-700 bg-transparent focus:ring-2 focus:ring-emerald-500 outline-none transition-all">
        </div>
        <div>
          <label class="block text-sm font-bold mb-2">{{ t('bookings.time') }}</label>
          <input v-model="form.time" type="time" required class="w-full px-4 py-3 rounded-xl border border-gray-200 dark:border-gray-700 bg-transparent focus:ring-2 focus:ring-emerald-500 outline-none transition-all">
        </div>
      </div>

      <div>
        <label class="block text-sm font-bold mb-2">{{ t('bookings.guests') }}</label>
        <select v-model="form.guests" class="w-full px-4 py-3 rounded-xl border border-gray-200 dark:border-gray-700 bg-transparent focus:ring-2 focus:ring-emerald-500 outline-none transition-all">
          <option v-for="n in 10" :key="n" :value="n">{{ n }} {{ t('bookings.people') }}</option>
        </select>
      </div>

      <BaseButton type="submit" variant="primary" size="lg" class="w-full" :disabled="loading">
        <Loader2 v-if="loading" class="w-5 h-5 animate-spin mr-2" />
        {{ t('bookings.submit') }}
      </BaseButton>
    </form>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { Loader2 } from 'lucide-vue-next';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const loading = ref(false);

const form = reactive({
  date: '',
  time: '',
  guests: 2
});

const handleBooking = async () => {
  loading.value = true;
  setTimeout(() => loading.value = false, 1000);
};
</script>