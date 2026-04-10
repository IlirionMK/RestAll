<template>
  <div class="space-y-8">
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <div v-for="stat in stats" :key="stat.label" class="bg-white dark:bg-gray-800 p-6 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-700">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400 font-medium">{{ t(stat.label) }}</p>
            <h3 class="text-2xl font-bold text-gray-900 dark:text-white mt-1">{{ stat.value }}</h3>
          </div>
          <div :class="`p-3 rounded-xl ${stat.color} bg-opacity-10 text-opacity-100`">
            <component :is="stat.icon" class="w-6 h-6" :class="stat.textColor" />
          </div>
        </div>
      </div>
    </div>

    <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
      <div class="p-6 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center">
        <h3 class="font-bold text-gray-900 dark:text-white">{{ t('admin.tables.title') }}</h3>
        <button class="text-sm text-emerald-600 font-bold hover:underline">View All</button>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead class="bg-gray-50 dark:bg-gray-700/50 text-gray-500 dark:text-gray-400 text-xs uppercase font-bold">
          <tr>
            <th class="px-6 py-4">{{ t('admin.tables.number') }}</th>
            <th class="px-6 py-4">{{ t('admin.tables.waiter') }}</th>
            <th class="px-6 py-4">{{ t('admin.tables.status') }}</th>
            <th class="px-6 py-4 text-right">{{ t('admin.tables.total') }}</th>
          </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 dark:divide-gray-700">
          <tr v-for="table in mockTables" :key="table.id" class="hover:bg-gray-50 dark:hover:bg-gray-700/30 transition-colors">
            <td class="px-6 py-4 font-bold text-gray-900 dark:text-white">#{{ table.id }}</td>
            <td class="px-6 py-4 text-gray-600 dark:text-gray-300">{{ table.waiter }}</td>
            <td class="px-6 py-4">
                <span :class="getStatusClass(table.status)" class="px-3 py-1 rounded-full text-xs font-bold">
                  {{ table.status }}
                </span>
            </td>
            <td class="px-6 py-4 text-right font-mono font-bold text-emerald-600">{{ table.total }}</td>
          </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { Banknote, Users, Clock } from 'lucide-vue-next';

const { t } = useI18n();

const stats = [
  { label: 'admin.stats.revenue', value: '1,240.50 PLN', icon: Banknote, color: 'bg-emerald-500', textColor: 'text-emerald-500' },
  { label: 'admin.stats.open_checks', value: '12', icon: Clock, color: 'bg-amber-500', textColor: 'text-amber-500' },
  { label: 'admin.stats.guests', value: '48', icon: Users, color: 'bg-blue-500', textColor: 'text-blue-500' },
];

const mockTables = [
  { id: 4, waiter: 'Marek S.', status: 'Eating', total: '145.00' },
  { id: 7, waiter: 'Anna K.', status: 'Waiting', total: '0.00' },
  { id: 2, waiter: 'Marek S.', status: 'Bill', total: '320.40' },
];

const getStatusClass = (status: string) => {
  switch (status) {
    case 'Eating': return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-400';
    case 'Waiting': return 'bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-400';
    case 'Bill': return 'bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-400';
    default: return 'bg-gray-100 text-gray-700';
  }
};
</script>