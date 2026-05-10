<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 md:py-20">
    <header class="flex flex-col md:flex-row md:items-end justify-between gap-6 mb-12 animate-fade-in">
      <div class="text-left">
        <h1 class="text-5xl md:text-8xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase leading-[0.85]">
          {{ t('bookings.my_title') }} <span class="text-restall-gold">{{ t('bookings.my_title_alt') }}</span>
        </h1>
        <p class="text-gray-500 font-medium tracking-tight mt-4">{{ t('bookings.dashboard_subtitle') }}</p>
      </div>
      <BaseButton :to="{ name: 'BookingForm' }" variant="ghost" class="w-fit rounded-full px-10 border-gray-200 no-underline">
        {{ t('bookings.new_btn') }}
      </BaseButton>
    </header>

    <div class="grid grid-cols-1 lg:grid-cols-4 gap-4 mb-12 animate-fade-in text-left">
      <div class="relative lg:col-span-1">
        <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
        <input
            v-model="filters.search"
            type="text"
            :placeholder="t('bookings.search_restaurant_placeholder')"
            class="w-full pl-10 pr-4 py-4 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-sm focus:border-restall-gold transition-colors dark:text-white"
        >
      </div>

      <div class="relative">
        <select
            v-model="filters.status"
            class="w-full px-4 py-4 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-sm appearance-none cursor-pointer focus:border-restall-gold transition-colors dark:text-white"
        >
          <option value="all">{{ t('bookings.filter_all_status') }}</option>
          <option value="pending">{{ t('bookings.status_pending') }}</option>
          <option value="confirmed">{{ t('bookings.status_confirmed') }}</option>
          <option value="cancelled">{{ t('bookings.status_cancelled') }}</option>
          <option value="completed">{{ t('bookings.status_completed') }}</option>
        </select>
        <ChevronDown class="absolute right-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
      </div>

      <div class="relative">
        <CalendarIcon class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        <input
            v-model="filters.date"
            type="date"
            class="w-full pl-10 pr-4 py-4 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-sm focus:border-restall-gold transition-colors dark:text-white"
        >
      </div>

      <div class="relative">
        <select
            v-model="filters.sort"
            class="w-full px-4 py-4 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-sm appearance-none cursor-pointer focus:border-restall-gold transition-colors dark:text-white"
        >
          <option value="desc">{{ t('bookings.sort_newest') }}</option>
          <option value="asc">{{ t('bookings.sort_oldest') }}</option>
        </select>
        <ListFilter class="absolute right-4 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
      </div>
    </div>

    <div v-if="!loading && filteredBookings.length === 0" class="flex flex-col items-center justify-center py-32 bg-gray-50 dark:bg-gray-900/30 rounded-[4rem] border-2 border-dashed border-gray-100 dark:border-gray-800">
      <CalendarX2 class="w-16 h-16 text-gray-200 mb-6" />
      <h3 class="text-2xl font-black text-restall-dark dark:text-restall-light uppercase tracking-tight">{{ t('bookings.empty_title') }}</h3>
      <BaseButton v-if="hasActiveFilters" @click="resetFilters" variant="link" class="mt-2 text-restall-gold no-underline font-bold">
        Clear filters
      </BaseButton>
      <BaseButton v-else :to="{ name: 'BookingForm' }" variant="link" class="mt-2 text-restall-gold no-underline font-bold">
        {{ t('bookings.empty_link') }}
      </BaseButton>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-8 animate-fade-in-up">
      <BookingCard
          v-for="booking in filteredBookings"
          :key="booking.id"
          :booking="booking"
      >
        <template #actions>
          <template v-if="['pending', 'confirmed'].includes(booking.status)">
            <BaseButton
                variant="primary"
                class="w-full rounded-2xl py-4 font-black text-[11px] uppercase tracking-widest"
                @click="openOrderModal(booking)"
            >
              {{ t('order.start_order') }}
            </BaseButton>
            <BaseButton
                @click="cancelBooking(booking.id)"
                variant="ghost"
                class="w-full rounded-2xl py-3 text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10 border-none font-black text-[11px] uppercase tracking-widest"
                :disabled="processingId === booking.id"
            >
              <Loader2 v-if="processingId === booking.id" class="w-4 h-4 animate-spin mr-2" />
              {{ t('bookings.cancel_btn') }}
            </BaseButton>
          </template>
          <div v-else class="w-full py-4 text-center text-[10px] font-black uppercase text-gray-400 bg-gray-50 dark:bg-gray-900/30 rounded-2xl tracking-widest">
            {{ t('bookings.archived_label') }}
          </div>
        </template>
      </BookingCard>
    </div>

    <ReservationOrderModal
        v-if="selectedBooking"
        v-model="orderModalOpen"
        :booking="selectedBooking"
    />

    <div v-if="loading" class="fixed inset-0 bg-white/80 dark:bg-black/80 backdrop-blur-md flex items-center justify-center z-50">
      <Loader2 class="w-16 h-16 animate-spin text-restall-gold" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import {
  Loader2,
  CalendarX2,
  Search,
  ChevronDown,
  Calendar as CalendarIcon,
  ListFilter
} from 'lucide-vue-next';
import { ReservationsService } from '@/api/reservations.service';
import BookingCard from '@/components/Booking/BookingCard.vue';
import ReservationOrderModal from '@/components/Booking/ReservationOrderModal.vue';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const bookings = ref<any[]>([]);
const loading = ref(true);
const processingId = ref<number | null>(null);
const orderModalOpen = ref(false);
const selectedBooking = ref<any>(null);

const openOrderModal = (booking: any) => {
  selectedBooking.value = booking;
  orderModalOpen.value = true;
};

const filters = reactive({
  search: '',
  status: 'all',
  date: '',
  sort: 'desc'
});

const loadData = async () => {
  loading.value = true;
  try {
    const response = await ReservationsService.index();
    bookings.value = Array.isArray(response.data) ? response.data : (Array.isArray(response) ? response : []);
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const hasActiveFilters = computed(() => {
  return filters.search !== '' || filters.status !== 'all' || filters.date !== '';
});

const resetFilters = () => {
  filters.search = '';
  filters.status = 'all';
  filters.date = '';
};

const filteredBookings = computed(() => {
  let result = [...bookings.value];

  if (filters.search) {
    const q = filters.search.toLowerCase();
    result = result.filter(b =>
        b.restaurant?.name?.toLowerCase().includes(q) ||
        String(b.id).includes(q)
    );
  }

  if (filters.status !== 'all') {
    result = result.filter(b => b.status === filters.status);
  }

  if (filters.date) {
    result = result.filter(b => b.reservation_time?.startsWith(filters.date));
  }

  result.sort((a, b) => {
    const timeA = new Date(a.reservation_time).getTime();
    const timeB = new Date(b.reservation_time).getTime();
    return filters.sort === 'desc' ? timeB - timeA : timeA - timeB;
  });

  return result;
});

const cancelBooking = async (id: number) => {
  if (!window.confirm(t('bookings.confirm_cancel'))) return;
  processingId.value = id;
  try {
    await ReservationsService.destroy(id);
    await loadData();
  } catch (e) {
    console.error(e);
  } finally {
    processingId.value = null;
  }
};

onMounted(loadData);
</script>