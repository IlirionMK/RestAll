<template>
  <div class="space-y-10 font-sans">
    <div>
      <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
        Dashboard
      </h1>
      <p class="text-gray-500 dark:text-gray-400 font-medium mt-2">
        Overview of your restaurant's performance.
      </p>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <Loader2 class="w-10 h-10 animate-spin text-restall-gold" />
    </div>

    <template v-else>
      <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
        <BaseStatCard title="Revenue Today" :value="formatCurrency(summary.revenue.today)" :trend="null">
          <template #icon><DollarSign class="w-6 h-6" /></template>
        </BaseStatCard>
        <BaseStatCard title="Revenue This Month" :value="formatCurrency(summary.revenue.this_month)" :trend="null">
          <template #icon><TrendingUp class="w-6 h-6" /></template>
        </BaseStatCard>
        <BaseStatCard title="Reservations Today" :value="summary.reservations.today" :trend="null">
          <template #icon><Calendar class="w-6 h-6" /></template>
        </BaseStatCard>
        <BaseStatCard title="Avg. Order Value" :value="formatCurrency(summary.orders.average_value)" :trend="null">
          <template #icon><ShoppingBag class="w-6 h-6" /></template>
        </BaseStatCard>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div class="lg:col-span-2 bg-white dark:bg-gray-800 p-8 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700">
          <h2 class="text-2xl font-black text-restall-dark dark:text-restall-light mb-6">Top Menu Items</h2>
          <div v-if="summary.top_items.length === 0" class="py-12 text-center text-gray-400 font-medium border-2 border-dashed border-gray-100 dark:border-gray-700 rounded-2xl">
            No data yet
          </div>
          <table v-else class="w-full text-left">
            <thead>
              <tr>
                <th class="pb-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Item</th>
                <th class="pb-4 text-[10px] font-black uppercase tracking-widest text-gray-400 text-right">Sold</th>
                <th class="pb-4 text-[10px] font-black uppercase tracking-widest text-gray-400 text-right">Revenue</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-50 dark:divide-gray-800">
              <tr v-for="item in summary.top_items" :key="item.name">
                <td class="py-3 font-bold text-restall-dark dark:text-restall-light">{{ item.name }}</td>
                <td class="py-3 font-bold text-gray-500 text-right">{{ item.quantity_sold }}</td>
                <td class="py-3 font-black text-restall-gold text-right">{{ formatCurrency(item.revenue) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="bg-white dark:bg-gray-800 p-8 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700">
          <h2 class="text-2xl font-black text-restall-dark dark:text-restall-light mb-6">This Week</h2>
          <div class="space-y-6">
            <div class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 rounded-2xl">
              <div>
                <p class="text-[10px] font-black uppercase text-gray-400 tracking-widest mb-1">Orders</p>
                <p class="text-2xl font-black text-restall-dark dark:text-restall-light">{{ summary.orders.this_week }}</p>
              </div>
              <ShoppingBag class="w-8 h-8 text-restall-green opacity-40" />
            </div>
            <div class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 rounded-2xl">
              <div>
                <p class="text-[10px] font-black uppercase text-gray-400 tracking-widest mb-1">Reservations</p>
                <p class="text-2xl font-black text-restall-dark dark:text-restall-light">{{ summary.reservations.this_week }}</p>
              </div>
              <Calendar class="w-8 h-8 text-restall-gold opacity-40" />
            </div>
            <div class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 rounded-2xl">
              <div>
                <p class="text-[10px] font-black uppercase text-gray-400 tracking-widest mb-1">Revenue</p>
                <p class="text-2xl font-black text-restall-dark dark:text-restall-light">{{ formatCurrency(summary.revenue.this_week) }}</p>
              </div>
              <DollarSign class="w-8 h-8 text-restall-green opacity-40" />
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { DollarSign, Calendar, ShoppingBag, TrendingUp, Loader2 } from 'lucide-vue-next';
import { API } from '@/api';
import type { AnalyticsSummary } from '@/api/analytics.service';
import BaseStatCard from '@/components/UI/BaseStatCard.vue';

const loading = ref(true);
const summary = ref<AnalyticsSummary>({
  revenue: { today: 0, this_week: 0, this_month: 0 },
  orders: { today: 0, this_week: 0, this_month: 0, average_value: 0 },
  top_items: [],
  reservations: { today: 0, this_week: 0 },
});

const formatCurrency = (value: number) =>
    value.toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });

onMounted(async () => {
  try {
    const { data } = await API.analytics.summary();
    summary.value = data;
  } finally {
    loading.value = false;
  }
});
</script>
