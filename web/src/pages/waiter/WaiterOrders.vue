<template>
  <div class="space-y-6">
    <div>
      <h1 class="text-3xl font-black uppercase tracking-tighter text-[#1F2937]">{{ t('waiter.history_title') }}</h1>
      <p class="text-sm text-[#1F2937]/50 font-medium mt-1">
        {{ new Date().toLocaleDateString(locale === 'pl' ? 'pl-PL' : 'en-GB', { weekday: 'long', day: 'numeric', month: 'long' }) }}
      </p>
    </div>

    <div class="flex flex-col sm:flex-row gap-3">
      <div class="flex items-center gap-1 bg-white rounded-2xl p-1 border border-gray-100 shadow-sm">
        <button
            v-for="f in filters"
            :key="f.key"
            @click="setStatus(f.key)"
            :class="[
              'px-4 py-2 rounded-xl text-[11px] font-black uppercase tracking-widest transition-colors',
              activeStatus === f.key ? 'bg-[#14532D] text-white shadow-sm' : 'text-[#1F2937]/50 hover:text-[#1F2937] hover:bg-gray-50'
            ]"
        >
          {{ f.label }}
        </button>
      </div>

      <div class="flex items-center gap-2 bg-white border border-gray-100 rounded-2xl px-4 py-2 shadow-sm">
        <CalendarIcon class="w-4 h-4 text-[#1F2937]/40 flex-shrink-0" />
        <input
            v-model="activeDate"
            type="date"
            class="text-sm font-medium text-[#1F2937] bg-transparent focus:outline-none"
        />
        <button v-if="activeDate" @click="activeDate = ''" class="p-0.5 rounded-lg hover:bg-gray-100 transition-colors">
          <X class="w-3.5 h-3.5 text-[#1F2937]/40" />
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-8 h-8 border-2 border-[#14532D] border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else>
      <div v-if="orders.length === 0" class="py-20 text-center border-2 border-dashed border-gray-200 rounded-3xl">
        <p class="text-sm font-black uppercase tracking-widest text-[#1F2937]/30">{{ t('waiter.no_orders') }}</p>
      </div>

      <div v-else class="space-y-2">
        <div
            v-for="order in orders"
            :key="order.id"
            class="bg-white border border-gray-100 rounded-2xl px-5 py-4 flex items-center gap-4 shadow-sm hover:shadow-md transition-shadow"
        >
          <div class="w-10 h-10 rounded-2xl bg-[#14532D]/10 flex items-center justify-center flex-shrink-0">
            <span class="text-[10px] font-black text-[#14532D]">#{{ order.id }}</span>
          </div>

          <div class="flex-1 min-w-0">
            <p class="font-black text-[#1F2937]">
              {{ order.table ? `${t('notif.table')} ${order.table.number}` : t('notif.takeaway') }}
            </p>
            <p class="text-xs text-[#1F2937]/50 font-medium mt-0.5">{{ formatDateTime(order.created_at) }}</p>
          </div>

          <div class="flex items-center gap-3">
            <span :class="['px-3 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest', statusClass(order.status)]">
              {{ statusLabel(order.status) }}
            </span>
            <p class="font-black text-[#1F2937] text-sm flex-shrink-0">{{ formatPrice(order.total_amount) }}</p>
          </div>
        </div>
      </div>

      <div v-if="lastPage > 1" class="flex items-center justify-center gap-2 pt-2">
        <button
            @click="changePage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="w-9 h-9 rounded-xl bg-white border border-gray-100 shadow-sm flex items-center justify-center text-[#1F2937]/50 hover:text-[#1F2937] disabled:opacity-30 transition-colors"
        >
          <ChevronLeft class="w-4 h-4" />
        </button>
        <span class="text-sm font-black text-[#1F2937]/50 px-2">{{ currentPage }} / {{ lastPage }}</span>
        <button
            @click="changePage(currentPage + 1)"
            :disabled="currentPage === lastPage"
            class="w-9 h-9 rounded-xl bg-white border border-gray-100 shadow-sm flex items-center justify-center text-[#1F2937]/50 hover:text-[#1F2937] disabled:opacity-30 transition-colors"
        >
          <ChevronRight class="w-4 h-4" />
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { CalendarIcon, X, ChevronLeft, ChevronRight } from 'lucide-vue-next';
import { API } from '@/api';
import { usePreferencesStore } from '@/stores/preferences.store';

interface OrderRow {
  id: number;
  status: string;
  total_amount: number | string;
  is_takeaway: boolean;
  created_at: string;
  table: { id: number; number: string | number } | null;
}

const { t } = useI18n();
const prefStore = usePreferencesStore();
const locale = computed(() => prefStore.locale);

const loading = ref(false);
const orders = ref<OrderRow[]>([]);
const currentPage = ref(1);
const lastPage = ref(1);
const activeStatus = ref('');
const activeDate = ref('');

const filters = computed(() => [
  { key: '',                  label: t('waiter.filter_all') },
  { key: 'pending',           label: t('waiter.filter_pending') },
  { key: 'billing_requested', label: t('waiter.filter_billing') },
  { key: 'paid',              label: t('waiter.filter_paid') },
]);

const statusClass = (status: string) => ({
  pending:           'bg-[#C29B40]/10 text-[#C29B40]',
  billing_requested: 'bg-blue-100 text-blue-600',
  paid:              'bg-[#14532D]/10 text-[#14532D]',
}[status] ?? 'bg-gray-100 text-gray-500');

const statusLabel = (status: string) => ({
  pending:           t('waiter.filter_pending'),
  billing_requested: t('waiter.filter_billing'),
  paid:              t('waiter.filter_paid'),
}[status] ?? status);

const formatPrice = (value: number | string) =>
    Number(value).toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });

const formatDateTime = (dateStr: string) =>
    new Date(dateStr).toLocaleString(locale.value === 'pl' ? 'pl-PL' : 'en-GB', {
      day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit',
    });

const fetchOrders = async () => {
  loading.value = true;
  try {
    const params: Record<string, any> = { history: 1, page: currentPage.value };
    if (activeStatus.value) params.status = activeStatus.value;
    if (activeDate.value)   params.date   = activeDate.value;

    const { data } = await API.orders.index(params);
    orders.value  = data.data ?? [];
    currentPage.value = data.current_page ?? 1;
    lastPage.value    = data.last_page ?? 1;
  } finally {
    loading.value = false;
  }
};

const setStatus = (key: string) => {
  activeStatus.value = key;
  currentPage.value  = 1;
};

const changePage = (page: number) => {
  currentPage.value = page;
};

watch([activeStatus, activeDate, currentPage], fetchOrders);

onMounted(fetchOrders);
</script>
