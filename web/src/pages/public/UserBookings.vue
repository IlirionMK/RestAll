<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 md:py-20">
    <header class="flex flex-col md:flex-row md:items-end justify-between gap-6 mb-16 animate-fade-in">
      <div class="text-left">
        <h1 class="text-5xl md:text-8xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase leading-[0.85]">
          {{ t('bookings.my_title') }} <span class="text-restall-gold">{{ t('bookings.my_title_alt') }}</span>
        </h1>
        <p class="text-gray-500 font-medium tracking-tight mt-4">{{ t('bookings.dashboard_subtitle') }}</p>
      </div>
      <BaseButton :to="{ name: 'Home' }" variant="ghost" class="w-fit rounded-full px-10 border-gray-200 no-underline">
        {{ t('bookings.new_btn') }}
      </BaseButton>
    </header>

    <div v-if="!loading && bookings.length === 0" class="flex flex-col items-center justify-center py-32 bg-gray-50 dark:bg-gray-900/30 rounded-[4rem] border-2 border-dashed border-gray-100 dark:border-gray-800">
      <CalendarX2 class="w-16 h-16 text-gray-200 mb-6" />
      <h3 class="text-2xl font-black text-restall-dark dark:text-restall-light uppercase tracking-tight">{{ t('bookings.empty_title') }}</h3>
      <BaseButton :to="{ name: 'Home' }" variant="link" class="mt-2 text-restall-gold no-underline font-bold">{{ t('bookings.empty_link') }}</BaseButton>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-8 animate-fade-in-up">
      <BookingCard
          v-for="booking in bookings"
          :key="booking.id"
          :booking="booking"
      >
        <template #actions>
          <BaseButton
              v-if="['pending', 'confirmed'].includes(booking.status)"
              @click="cancelBooking(booking.id)"
              variant="ghost"
              class="w-full rounded-2xl py-4 text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10 border-none font-black text-[11px] uppercase tracking-widest"
              :disabled="processingId === booking.id"
          >
            <Loader2 v-if="processingId === booking.id" class="w-4 h-4 animate-spin mr-2" />
            {{ t('bookings.cancel_btn') }}
          </BaseButton>
          <div v-else class="w-full py-4 text-center text-[10px] font-black uppercase text-gray-400 bg-gray-50 dark:bg-gray-900/30 rounded-2xl tracking-widest">
            {{ t('bookings.archived_label') }}
          </div>
        </template>
      </BookingCard>
    </div>

    <div v-if="loading" class="fixed inset-0 bg-white/80 dark:bg-black/80 backdrop-blur-md flex items-center justify-center z-50">
      <Loader2 class="w-16 h-16 animate-spin text-restall-gold" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { Loader2, CalendarX2 } from 'lucide-vue-next';
import { ReservationsService } from '@/api/reservations.service';
import BookingCard from '@/components/Booking/BookingCard.vue';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const bookings = ref<any[]>([]);
const loading = ref(true);
const processingId = ref<number | null>(null);

const loadData = async () => {
  try { bookings.value = await ReservationsService.index(); }
  catch (e) { console.error(e); } finally { loading.value = false; }
};

const cancelBooking = async (id: number) => {
  if (!window.confirm(t('bookings.confirm_cancel'))) return;
  processingId.value = id;
  try {
    await ReservationsService.destroy(id);
    await loadData();
  } catch (e) { console.error(e); } finally { processingId.value = null; }
};

onMounted(loadData);
</script>