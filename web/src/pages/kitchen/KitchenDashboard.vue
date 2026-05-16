<template>
  <div class="h-full">
    <div v-if="loading" class="flex items-center justify-center h-64">
      <div class="w-8 h-8 border-2 border-[#C29B40] border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else>
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center gap-3">
          <div :class="['w-2 h-2 rounded-full', connected ? 'bg-[#14532D] animate-pulse' : 'bg-gray-600']" />
          <span class="text-[10px] font-black uppercase tracking-widest text-gray-400">
            {{ connected ? 'Live' : 'Connecting...' }}
          </span>
        </div>
        <span class="text-[10px] font-black uppercase tracking-widest text-gray-500">
          {{ totalActive }} active ticket{{ totalActive !== 1 ? 's' : '' }}
        </span>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 min-h-[60vh]">
        <div
            v-for="column in columns"
            :key="column.status"
            class="flex flex-col gap-4"
        >
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-[11px] font-black uppercase tracking-widest" :style="{ color: column.color }">
              {{ column.label }}
            </h3>
            <span class="px-2 py-0.5 rounded-full text-[10px] font-black text-gray-400 bg-gray-800">
              {{ byStatus(column.status).length }}
            </span>
          </div>

          <div
              v-if="byStatus(column.status).length === 0"
              class="flex-1 flex items-center justify-center border-2 border-dashed border-gray-700 rounded-3xl text-gray-600 text-[10px] font-black uppercase tracking-widest min-h-[120px]"
          >
            Empty
          </div>

          <div
              v-for="ticket in byStatus(column.status)"
              :key="ticket.id"
              class="bg-gray-800 rounded-2xl p-5 border border-gray-700/50 flex flex-col gap-3"
          >
            <div class="flex items-start justify-between gap-2">
              <div>
                <p class="font-black text-white leading-tight">{{ ticket.name }}</p>
                <p class="text-[10px] font-black uppercase tracking-widest mt-1" :style="{ color: column.color }">
                  x{{ ticket.quantity }}
                </p>
              </div>
              <div class="text-right flex-shrink-0">
                <p v-if="ticket.table_number" class="text-[10px] font-black uppercase text-[#C29B40]">
                  Table {{ ticket.table_number }}
                </p>
                <p v-else class="text-[10px] font-black uppercase text-gray-500">Take Away</p>
              </div>
            </div>

            <div class="flex items-center justify-between">
              <span class="text-[10px] text-gray-500 font-medium">{{ formatTime(ticket.created_at) }}</span>
              <button
                  v-if="column.next"
                  @click="advance(ticket)"
                  :disabled="advancingId === ticket.id"
                  :style="{ backgroundColor: column.color + '20', color: column.color }"
                  class="px-3 py-1.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-opacity disabled:opacity-40 hover:opacity-80"
              >
                <span v-if="advancingId === ticket.id">...</span>
                <span v-else>{{ column.nextLabel }}</span>
              </button>
              <span v-else class="text-[10px] font-black uppercase text-gray-600">Done</span>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { API } from '@/api';
import { echo } from '@/echo';
import { useAuthStore } from '@/stores/auth.store';

interface KitchenTicket {
  id: number;
  order_id: number;
  table_number: number | null;
  name: string;
  quantity: number;
  status: 'pending' | 'preparing' | 'ready';
  created_at: string;
  is_takeaway?: boolean;
}

const authStore = useAuthStore();
const loading = ref(true);
const connected = ref(false);
const tickets = ref<KitchenTicket[]>([]);
const advancingId = ref<number | null>(null);

const columns = [
  { status: 'pending', label: 'Pending', color: '#C29B40', next: 'preparing', nextLabel: 'Start' },
  { status: 'preparing', label: 'Preparing', color: '#3B82F6', next: 'ready', nextLabel: 'Ready' },
  { status: 'ready', label: 'Ready', color: '#14532D', next: null, nextLabel: '' },
] as const;

const byStatus = (status: string) => tickets.value.filter(t => t.status === status);

const totalActive = computed(() =>
    tickets.value.filter(t => t.status !== 'ready').length
);

const formatTime = (dateStr: string) =>
    new Date(dateStr).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' });

const fetchTickets = async () => {
  loading.value = true;
  try {
    const result = await API.kitchen.getTickets();
    tickets.value = result.data ?? result;
  } finally {
    loading.value = false;
  }
};

const advance = async (ticket: KitchenTicket) => {
  const col = columns.find(c => c.status === ticket.status);
  if (!col?.next) return;

  advancingId.value = ticket.id;
  try {
    await API.kitchen.updateStatus(ticket.id, col.next);
    ticket.status = col.next as KitchenTicket['status'];
  } finally {
    advancingId.value = null;
  }
};

const handleItemsAdded = (data: any) => {
  const incoming: KitchenTicket[] = data.items ?? data.tickets ?? [];
  incoming.forEach(item => {
    if (!tickets.value.find(t => t.id === item.id)) {
      tickets.value.push(item);
    }
  });
};

const handleStatusUpdated = (data: any) => {
  const id = data.id ?? data.ticket_id;
  const status = data.status;
  const ticket = tickets.value.find(t => t.id === id);
  if (ticket && status) {
    ticket.status = status;
  }
};

let kitchenChannel: any = null;

onMounted(async () => {
  await fetchTickets();

  const restaurantId = authStore.user?.restaurant_id;
  if (!restaurantId) return;

  kitchenChannel = echo.private(`restaurant.${restaurantId}.kitchen`);
  kitchenChannel
      .listen('.kitchen.items_added', handleItemsAdded)
      .listen('.kitchen.ticket_status_updated', handleStatusUpdated);

  connected.value = true;
});

onUnmounted(() => {
  kitchenChannel?.stopListening('.kitchen.items_added', handleItemsAdded);
  kitchenChannel?.stopListening('.kitchen.ticket_status_updated', handleStatusUpdated);
});
</script>
