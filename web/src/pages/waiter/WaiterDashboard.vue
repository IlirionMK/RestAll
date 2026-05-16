<template>
  <div class="space-y-10">

    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-black uppercase tracking-tighter text-[#1F2937]">{{ t('waiter.dashboard') }}</h1>
        <p class="text-sm text-[#1F2937]/50 font-medium mt-1">
          {{ new Date().toLocaleDateString(locale === 'pl' ? 'pl-PL' : 'en-GB', { weekday: 'long', day: 'numeric', month: 'long' }) }}
        </p>
      </div>
      <div class="flex items-center gap-2">
        <div :class="['w-2 h-2 rounded-full', connected ? 'bg-[#14532D] animate-pulse' : 'bg-gray-300']" />
        <span class="text-[10px] font-black uppercase tracking-widest text-[#1F2937]/40">
          {{ connected ? 'Live' : 'Connecting...' }}
        </span>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-8 h-8 border-2 border-[#14532D] border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else>

      <section v-if="billingOrders.length > 0">
        <div class="flex items-center gap-3 mb-4">
          <div class="w-2 h-2 rounded-full bg-[#C29B40] animate-pulse" />
          <h2 class="text-[11px] font-black uppercase tracking-widest text-[#C29B40]">
            {{ t('order.bill_requested') }} — {{ billingOrders.length }}
          </h2>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div v-for="order in billingOrders" :key="order.id"
               class="bg-white border border-[#C29B40]/30 rounded-2xl p-5 flex flex-col gap-4 shadow-sm">
            <div class="flex items-start justify-between">
              <div>
                <p class="text-[10px] font-black uppercase tracking-widest text-[#C29B40] mb-1">{{ t('order.status_billing_requested') }}</p>
                <p class="text-2xl font-black text-[#1F2937]">
                  {{ order.table ? `${t('order.table')} ${order.table.number}` : t('order.takeaway_label') }}
                </p>
                <p class="text-xs text-[#1F2937]/40 font-medium mt-0.5">{{ timeAgo(order.created_at) }}</p>
              </div>
              <p class="text-xl font-black text-[#C29B40]">{{ formatPrice(order.total_amount) }}</p>
            </div>
            <button @click="markPaid(order)" :disabled="payingId === order.id"
                    class="w-full py-3 bg-[#14532D] text-white font-black text-xs uppercase tracking-widest rounded-xl hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center justify-center gap-2">
              <div v-if="payingId === order.id" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
              Mark as Paid
            </button>
          </div>
        </div>
      </section>

      <section v-if="todaysReservations.length > 0">
        <h2 class="text-[11px] font-black uppercase tracking-widest text-[#1F2937]/40 mb-4">
          {{ t('bookings.title') }} — {{ todaysReservations.length }}
        </h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div v-for="res in todaysReservations" :key="res.id"
               class="bg-white border border-gray-100 rounded-2xl p-4 flex items-center justify-between gap-4 shadow-sm">
            <div class="min-w-0">
              <p class="font-black text-[#1F2937] text-base">{{ t('order.table') }} {{ res.table?.number }}</p>
              <p class="text-xs text-[#1F2937]/50 font-medium">
                {{ formatTime(res.reservation_time) }} · {{ res.guests_count }} guests
              </p>
              <p v-if="res.user" class="text-xs text-[#1F2937]/40 font-medium truncate mt-0.5">{{ res.user.name }}</p>
            </div>
            <button @click="seatGuests(res)" :disabled="seatingId === res.id"
                    class="flex-shrink-0 px-4 py-2.5 bg-[#14532D] text-white font-black text-[10px] uppercase tracking-widest rounded-xl hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center gap-2">
              <div v-if="seatingId === res.id" class="w-3 h-3 border-2 border-white border-t-transparent rounded-full animate-spin" />
              <span v-else>Seat Guests</span>
            </button>
          </div>
        </div>
      </section>

      <section>
        <h2 class="text-[11px] font-black uppercase tracking-widest text-[#1F2937]/40 mb-4">
          {{ t('order.status_pending') }} — {{ pendingOrders.length }}
        </h2>

        <div v-if="pendingOrders.length === 0" class="py-12 text-center border-2 border-dashed border-gray-200 rounded-2xl text-[#1F2937]/30 text-sm font-black uppercase tracking-widest">
          No active orders
        </div>

        <div v-else class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
          <div v-for="order in pendingOrders" :key="order.id"
               class="bg-white border border-gray-100 rounded-2xl p-4 flex flex-col gap-2 shadow-sm">
            <p class="text-base font-black text-[#1F2937]">
              {{ order.table ? `${t('order.table')} ${order.table.number}` : t('order.takeaway_label') }}
            </p>
            <p class="text-[#C29B40] font-black text-sm">{{ formatPrice(order.total_amount) }}</p>
            <p class="text-[10px] text-[#1F2937]/40 font-medium">{{ timeAgo(order.created_at) }}</p>
            <div class="px-2 py-1 bg-gray-100 rounded-lg text-[9px] font-black uppercase tracking-widest text-[#1F2937]/40 text-center">
              {{ t('order.status_pending') }}
            </div>
          </div>
        </div>
      </section>

    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { API } from '@/api';
import { echo } from '@/echo';
import { useAuthStore } from '@/stores/auth.store';
import { useToast } from '@/composables/useToast';
import { usePreferencesStore } from '@/stores/preferences.store';
import { ReservationsService } from '@/api/reservations.service';

interface OrderTable { id: number; number: string | number; }
interface Order { id: number; status: string; total_amount: number | string; is_takeaway: boolean; created_at: string; table: OrderTable | null; }
interface Reservation { id: number; status: string; reservation_time: string; guests_count: number; table: OrderTable | null; user: { name: string } | null; }

const { t } = useI18n();
const authStore = useAuthStore();
const prefStore = usePreferencesStore();
const { success, error: toastError } = useToast();

const locale = computed(() => prefStore.locale);
const loading = ref(true);
const connected = ref(false);
const orders = ref<Order[]>([]);
const reservations = ref<Reservation[]>([]);
const payingId = ref<number | null>(null);
const seatingId = ref<number | null>(null);

const billingOrders = computed(() => orders.value.filter(o => o.status === 'billing_requested'));
const pendingOrders = computed(() => orders.value.filter(o => o.status === 'pending'));

const todaysReservations = computed(() => {
  const today = new Date().toDateString();
  return reservations.value.filter(r =>
      ['confirmed', 'pending'].includes(r.status) &&
      new Date(r.reservation_time).toDateString() === today
  );
});

const formatPrice = (value: number | string) =>
    Number(value).toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });

const formatTime = (dateStr: string) =>
    new Date(dateStr).toLocaleTimeString(locale.value === 'pl' ? 'pl-PL' : 'en-GB', { hour: '2-digit', minute: '2-digit' });

const timeAgo = (dateStr: string) => {
  const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 60000);
  if (diff < 1) return t('notif.just_now');
  return t('notif.min_ago', { n: diff });
};

const fetchOrders = async () => {
  const { data } = await API.orders.index();
  orders.value = Array.isArray(data) ? data : (data.data ?? []);
};

const fetchReservations = async () => {
  const data = await ReservationsService.index();
  reservations.value = Array.isArray(data) ? data : [];
};

const markPaid = async (order: Order) => {
  payingId.value = order.id;
  try {
    await API.orders.pay(order.id);
    orders.value = orders.value.filter(o => o.id !== order.id);
    success(`${t('order.table')} ${order.table?.number ?? t('order.takeaway_label')} — paid`);
  } catch {
    toastError('Failed to process payment');
  } finally {
    payingId.value = null;
  }
};

const seatGuests = async (res: Reservation) => {
  seatingId.value = res.id;
  try {
    await ReservationsService.createOrder(res.id, false);
    reservations.value = reservations.value.filter(r => r.id !== res.id);
    await fetchOrders();
    success(`${t('order.table')} ${res.table?.number} — order started`);
  } catch {
    toastError('Failed to start order');
  } finally {
    seatingId.value = null;
  }
};

const handleBillingRequested = (data: any) => {
  const id = data.order_id ?? data.id;
  const order = orders.value.find(o => o.id === id);
  if (order) {
    order.status = 'billing_requested';
  } else {
    fetchOrders();
  }
};

let channel: any = null;

onMounted(async () => {
  loading.value = true;
  try {
    await Promise.all([fetchOrders(), fetchReservations()]);
  } finally {
    loading.value = false;
  }

  const restaurantId = authStore.user?.restaurant_id;
  if (!restaurantId) return;

  channel = echo.private(`restaurant.${restaurantId}.staff`);
  channel.listen('.order.billing_requested', handleBillingRequested);
  connected.value = true;
});

onUnmounted(() => {
  channel?.stopListening('.order.billing_requested', handleBillingRequested);
});
</script>
