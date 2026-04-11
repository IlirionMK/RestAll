<template>
  <div class="max-w-4xl mx-auto px-6 py-12 md:py-20 font-sans">
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-16 items-start">

      <div class="space-y-8">
        <div>
          <h1 class="text-4xl md:text-5xl font-black text-gray-900 dark:text-white tracking-tighter leading-tight uppercase">
            {{ t('bookings.title') }}
          </h1>
          <p class="mt-4 text-lg text-gray-500 dark:text-gray-400 font-medium italic">
            {{ t('bookings.subtitle') }}
          </p>
        </div>

        <div class="space-y-6">
          <div class="flex items-start space-x-4">
            <div class="w-12 h-12 rounded-2xl bg-emerald-50 dark:bg-emerald-500/10 flex items-center justify-center flex-shrink-0">
              <MapPin class="w-6 h-6 text-emerald-600" />
            </div>
            <div>
              <h4 class="font-bold dark:text-white">Main Dining Hall</h4>
              <p class="text-sm text-gray-500">Elegant atmosphere with a view of the open kitchen.</p>
            </div>
          </div>

          <div class="flex items-start space-x-4">
            <div class="w-12 h-12 rounded-2xl bg-amber-50 dark:bg-amber-500/10 flex items-center justify-center flex-shrink-0">
              <Clock class="w-6 h-6 text-amber-600" />
            </div>
            <div>
              <h4 class="font-bold dark:text-white">Opening Hours</h4>
              <p class="text-sm text-gray-500">Mon-Sun: 12:00 PM - 11:00 PM</p>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white dark:bg-gray-900 p-8 md:p-10 rounded-[3rem] shadow-2xl shadow-emerald-900/5 border border-gray-100 dark:border-gray-800 relative overflow-hidden">
        <form @submit.prevent="handleBooking" class="space-y-8 relative z-10">
          <div class="space-y-2">
            <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 ml-1">
              {{ t('bookings.guests') }}
            </label>
            <div class="flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-800 rounded-2xl border border-transparent focus-within:border-emerald-500/30 transition-all">
              <button type="button" @click="form.guests > 1 && form.guests--" class="w-12 h-12 rounded-xl bg-white dark:bg-gray-700 shadow-sm flex items-center justify-center hover:text-emerald-600 transition-colors">
                <Minus class="w-5 h-5" />
              </button>
              <span class="text-xl font-black dark:text-white">{{ form.guests }}</span>
              <button type="button" @click="form.guests < 10 && form.guests++" class="w-12 h-12 rounded-xl bg-white dark:bg-gray-700 shadow-sm flex items-center justify-center hover:text-emerald-600 transition-colors">
                <Plus class="w-5 h-5" />
              </button>
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div class="space-y-2">
              <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 ml-1">
                {{ t('bookings.date') }}
              </label>
              <div class="relative">
                <Calendar class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                    v-model="form.date"
                    type="date"
                    required
                    class="w-full pl-12 pr-4 py-4 bg-gray-50 dark:bg-gray-800 border-none rounded-2xl focus:ring-2 focus:ring-emerald-500 outline-none transition-all dark:text-white font-bold"
                >
              </div>
            </div>

            <div class="space-y-2">
              <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 ml-1">
                {{ t('bookings.time') }}
              </label>
              <div class="relative">
                <Clock class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                    v-model="form.time"
                    type="time"
                    required
                    class="w-full pl-12 pr-4 py-4 bg-gray-50 dark:bg-gray-800 border-none rounded-2xl focus:ring-2 focus:ring-emerald-500 outline-none transition-all dark:text-white font-bold"
                >
              </div>
            </div>
          </div>

          <BaseButton
              type="submit"
              variant="primary"
              size="lg"
              class="w-full py-5 text-lg"
              :disabled="loading"
          >
            <Loader2 v-if="loading" class="w-5 h-5 animate-spin mr-2" />
            {{ t('bookings.submit') }}
          </BaseButton>
        </form>

        <div class="absolute -top-24 -right-24 w-48 h-48 bg-emerald-500/5 rounded-full blur-3xl"></div>
        <div class="absolute -bottom-24 -left-24 w-48 h-48 bg-amber-500/5 rounded-full blur-3xl"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import {
  Calendar, Clock, Plus, Minus,
  MapPin, Loader2
} from 'lucide-vue-next';
import { API } from '@/api';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const router = useRouter();
const loading = ref(false);

const form = reactive({
  date: new Date().toISOString().split('T')[0],
  time: '19:00',
  guests: 2,
  table_id: null
});

const handleBooking = async () => {
  loading.value = true;
  try {
    await API.reservations.create(form);
    router.push('/');
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
input[type="date"]::-webkit-calendar-picker-indicator,
input[type="time"]::-webkit-calendar-picker-indicator {
  opacity: 0;
  width: 100%;
  height: 100%;
  position: absolute;
  top: 0;
  left: 0;
  cursor: pointer;
}
</style>