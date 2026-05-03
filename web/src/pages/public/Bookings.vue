<template>
  <div class="max-w-6xl mx-auto px-4 sm:px-6 py-8 md:py-24 overflow-x-hidden">
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-20 items-start">

      <div class="space-y-12 animate-fade-in text-left">
        <div v-if="!isSuccess">
          <span class="inline-block py-1 px-4 rounded-full bg-restall-gold/20 text-restall-gold text-xs font-black uppercase tracking-widest mb-6">
            {{ t('bookings.step_info', { current: currentStep, total: 2 }) }}
          </span>
          <h1 class="text-5xl md:text-7xl font-black text-restall-dark dark:text-restall-light tracking-tighter leading-[0.9] uppercase">
            {{ currentStep === 1 ? t('bookings.title') : t('bookings.choose_table') }}
          </h1>
        </div>
        <div v-else>
          <span class="inline-block py-1 px-4 rounded-full bg-restall-green/20 text-restall-green text-xs font-black uppercase tracking-widest mb-6">
            {{ t('bookings.status_success') }}
          </span>
          <h1 class="text-5xl md:text-7xl font-black text-restall-dark dark:text-restall-light tracking-tighter leading-[0.9] uppercase">
            {{ t('bookings.see_you') }}
          </h1>
        </div>

        <div class="flex items-start space-x-6 group">
          <div class="w-14 h-14 rounded-2xl bg-restall-green/10 flex items-center justify-center flex-shrink-0">
            <MapPin class="w-6 h-6 text-restall-green" />
          </div>
          <div>
            <h4 class="text-lg font-black text-restall-dark dark:text-restall-light uppercase tracking-tight">RestAll Premium</h4>
            <p class="text-gray-500 font-medium">Main Avenue, 42</p>
          </div>
        </div>
      </div>

      <div class="bg-white dark:bg-gray-800/50 p-8 md:p-12 rounded-[3.5rem] shadow-2xl border border-gray-100 dark:border-gray-700 relative backdrop-blur-sm">

        <div v-if="isSuccess" class="text-center py-10 space-y-8 relative z-10 animate-fade-in-up">
          <div class="w-24 h-24 bg-restall-green/20 rounded-full flex items-center justify-center mx-auto mb-8">
            <Check class="w-12 h-12 text-restall-green" />
          </div>
          <div class="space-y-4">
            <h2 class="text-3xl font-black text-restall-dark dark:text-restall-light uppercase tracking-tight">{{ t('bookings.reserved_title') }}</h2>
            <p class="text-gray-500 font-medium max-w-xs mx-auto text-center leading-relaxed">
              {{ t('bookings.confirmed_text') }} <span class="text-restall-dark dark:text-restall-light font-bold">{{ date }}</span> {{ t('bookings.at') }} <span class="text-restall-dark dark:text-restall-light font-bold">{{ time }}</span>.
            </p>
          </div>
          <div class="pt-8 space-y-4">
            <BaseButton :to="{ name: 'Home' }" variant="primary" class="w-full rounded-full py-6 no-underline shadow-xl shadow-restall-gold/20">
              {{ t('nav.home') }}
            </BaseButton>
            <BaseButton :to="{ name: 'UserBookings' }" variant="ghost" class="w-full rounded-full no-underline">
              {{ t('bookings.view_my_btn') }}
            </BaseButton>
          </div>
        </div>

        <template v-else>
          <form v-if="currentStep === 1" @submit.prevent="goToStepTwo" class="space-y-10 relative z-10 animate-fade-in-up text-left">
            <div class="space-y-4">
              <label class="text-[11px] font-black uppercase tracking-[0.2em] text-gray-400 ml-1">{{ t('bookings.guests_label') }}</label>
              <div class="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-900/50 rounded-3xl border border-transparent focus-within:border-restall-green/30 transition-all">
                <button type="button" @click="form.guests_count > 1 && form.guests_count--" class="w-14 h-14 rounded-2xl bg-white dark:bg-gray-800 shadow-sm flex items-center justify-center active:scale-95 transition-transform"><Minus class="w-6 h-6" /></button>
                <div class="text-center">
                  <span class="text-3xl font-black text-restall-dark dark:text-restall-light">{{ form.guests_count }}</span>
                  <span class="block text-[10px] font-black uppercase text-gray-400">{{ t('bookings.people') }}</span>
                </div>
                <button type="button" @click="form.guests_count < 10 && form.guests_count++" class="w-14 h-14 rounded-2xl bg-white dark:bg-gray-800 shadow-sm flex items-center justify-center active:scale-95 transition-transform"><Plus class="w-6 h-6" /></button>
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-8">
              <div class="space-y-4 text-left">
                <label class="text-[11px] font-black uppercase tracking-[0.2em] text-gray-400 ml-1">{{ t('bookings.date_label') }}</label>
                <div class="relative">
                  <Calendar class="absolute left-5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 pointer-events-none" />
                  <input v-model="date" type="date" required class="w-full pl-14 pr-5 py-5 bg-gray-50 dark:bg-gray-900/50 border-none rounded-3xl outline-none font-bold text-lg cursor-pointer dark:text-white">
                </div>
              </div>
              <div class="space-y-4 text-left">
                <label class="text-[11px] font-black uppercase tracking-[0.2em] text-gray-400 ml-1">{{ t('bookings.time_label') }}</label>
                <div class="relative">
                  <Clock class="absolute left-5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 pointer-events-none" />
                  <input v-model="time" type="time" required class="w-full pl-14 pr-5 py-5 bg-gray-50 dark:bg-gray-900/50 border-none rounded-3xl outline-none font-bold text-lg cursor-pointer dark:text-white">
                </div>
              </div>
            </div>

            <BaseButton type="submit" size="lg" class="w-full py-6 text-xl rounded-full" :disabled="loading">
              {{ t('bookings.next_btn') }}
            </BaseButton>
          </form>

          <div v-else class="space-y-8 animate-fade-in-up text-left">
            <div v-if="bookingError" class="p-4 bg-rose-50 dark:bg-rose-500/10 border border-rose-200 dark:border-rose-500/20 rounded-2xl">
              <p class="text-rose-500 text-sm font-bold text-center uppercase tracking-tight">{{ bookingError }}</p>
            </div>

            <div v-if="tablesLoading" class="flex flex-col items-center py-12">
              <Loader2 class="w-10 h-10 animate-spin text-restall-gold mb-4" />
              <p class="text-gray-400 font-bold uppercase text-xs tracking-widest">{{ t('bookings.loading_tables') }}</p>
            </div>

            <div v-else class="grid grid-cols-2 gap-4 max-h-[400px] overflow-y-auto pr-2 custom-scrollbar">
              <button
                  v-for="table in availableTables" :key="table.id"
                  @click="table.status !== 'booked' && (form.table_id = table.id)"
                  :disabled="table.status === 'booked'"
                  :class="['p-6 rounded-[2rem] border-2 transition-all flex flex-col items-center justify-center gap-2 relative overflow-hidden', form.table_id === table.id ? 'border-restall-gold bg-restall-gold/5 scale-95 shadow-inner' : 'border-transparent bg-gray-50 dark:bg-gray-900/50', table.status === 'booked' ? 'opacity-40 grayscale cursor-not-allowed' : 'hover:bg-gray-100']"
              >
                <div v-if="table.status === 'booked'" class="absolute inset-0 flex items-center justify-center bg-black/5 backdrop-blur-[1px]">
                  <span class="text-[9px] font-black uppercase bg-white/90 dark:bg-gray-800/90 px-2 py-1 rounded shadow-sm tracking-tighter">{{ t('bookings.table_reserved') }}</span>
                </div>
                <span class="text-xs font-black text-gray-400 uppercase">{{ t('bookings.table_label_short') }}</span>
                <span class="text-2xl font-black text-restall-dark dark:text-restall-light">{{ table.number }}</span>
                <span class="text-[10px] font-bold text-restall-gold uppercase">{{ table.capacity }} {{ t('bookings.spots') }}</span>
              </button>
            </div>

            <div class="flex gap-4">
              <BaseButton variant="ghost" @click="currentStep = 1" class="flex-1 rounded-full" :disabled="loading">
                {{ t('common.back') }}
              </BaseButton>
              <BaseButton class="flex-[2] rounded-full shadow-xl" :disabled="!form.table_id || loading" @click="handleBooking">
                <Loader2 v-if="loading" class="w-6 h-6 animate-spin mr-3" />
                {{ t('bookings.confirm_btn') }}
              </BaseButton>
            </div>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import { Calendar, Clock, Plus, Minus, MapPin, Loader2, Check } from 'lucide-vue-next';
import { TablesService } from '@/api/tables.service';
import { ReservationsService } from '@/api/reservations.service';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const route = useRoute();

const loading = ref(false);
const tablesLoading = ref(false);
const isSuccess = ref(false);
const bookingError = ref<string | null>(null);
const currentStep = ref(1);
const restaurantId = Number(route.params.restaurantId);

const date = ref(new Date().toISOString().split('T')[0]);
const time = ref('19:00');

const form = reactive({ guests_count: 2, table_id: null as number | null });
const availableTables = ref<any[]>([]);

const fetchTables = async () => {
  tablesLoading.value = true;
  try {
    const response = await TablesService.getAll({
      restaurant_id: restaurantId,
      reservation_time: `${date.value} ${time.value}:00`
    });
    const tablesData = response.data || response;
    availableTables.value = tablesData.filter((t: any) => t.capacity >= form.guests_count);
  } catch (e) { console.error(e); } finally { tablesLoading.value = false; }
};

const goToStepTwo = async () => { currentStep.value = 2; await fetchTables(); };

const handleBooking = async () => {
  loading.value = true;
  bookingError.value = null;
  try {
    await ReservationsService.create({
      table_id: form.table_id,
      reservation_time: `${date.value} ${time.value}:00`,
      guests_count: form.guests_count
    });
    isSuccess.value = true;
  } catch (err: any) {
    if (err.response?.status === 422) {
      bookingError.value = t('errors.table_occupied');
      await fetchTables();
    } else {
      bookingError.value = t('errors.booking_failed');
    }
  } finally { loading.value = false; }
};
</script>

<style scoped>
input[type="date"]::-webkit-calendar-picker-indicator,
input[type="time"]::-webkit-calendar-picker-indicator {
  opacity: 0; width: 100%; height: 100%; position: absolute; top: 0; left: 0; cursor: pointer;
}
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(156,163,175,0.3); border-radius: 10px; }
@keyframes fade-in-up { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
.animate-fade-in-up { animation: fade-in-up 0.4s ease-out forwards; }
</style>