<template>
  <div class="space-y-10 font-sans">
    <div>
      <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
        System Logs
      </h1>
    </div>

    <div class="bg-white dark:bg-gray-800 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
      <div class="p-6 border-b border-gray-100 dark:border-gray-700 flex flex-wrap gap-4">
        <input
            v-model="filters.action"
            type="text"
            placeholder="Filter by action..."
            @keyup.enter="fetchLogs(1)"
            class="flex-1 min-w-[160px] px-4 py-3 bg-gray-50 dark:bg-gray-900 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light"
        />
        <input
            v-model="filters.date_from"
            type="date"
            class="px-4 py-3 bg-gray-50 dark:bg-gray-900 rounded-2xl outline-none font-bold text-gray-500 cursor-pointer"
        />
        <input
            v-model="filters.date_to"
            type="date"
            class="px-4 py-3 bg-gray-50 dark:bg-gray-900 rounded-2xl outline-none font-bold text-gray-500 cursor-pointer"
        />
        <BaseButton variant="primary" size="sm" class="rounded-2xl" @click="fetchLogs(1)">
          Search
        </BaseButton>
        <BaseButton variant="ghost" size="sm" class="rounded-2xl" @click="resetFilters">
          Reset
        </BaseButton>
      </div>

      <div v-if="loading" class="flex justify-center py-20">
        <Loader2 class="w-10 h-10 animate-spin text-restall-gold" />
      </div>

      <template v-else>
        <div v-if="logs.length === 0" class="py-16 text-center text-gray-400 font-medium">
          No logs found
        </div>

        <div v-else class="divide-y divide-gray-100 dark:divide-gray-800">
          <div
              v-for="log in logs"
              :key="log.id"
              class="flex items-start gap-4 p-5 hover:bg-gray-50 dark:hover:bg-gray-900/50 transition-colors"
          >
            <div class="w-10 h-10 rounded-xl flex-shrink-0 flex items-center justify-center bg-gray-100 dark:bg-gray-700 mt-0.5">
              <Terminal class="w-5 h-5 text-gray-500" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex flex-wrap items-center justify-between gap-2 mb-1">
                <div class="flex items-center gap-2 flex-wrap">
                  <span class="px-2.5 py-0.5 bg-gray-100 dark:bg-gray-700 text-restall-dark dark:text-restall-light rounded-lg text-[10px] font-black uppercase tracking-wider font-mono">
                    {{ log.action }}
                  </span>
                  <span v-if="log.user" class="text-sm font-bold text-gray-600 dark:text-gray-300">
                    {{ log.user.name }}
                  </span>
                  <span v-if="log.model_type" class="text-[10px] font-bold text-gray-400 uppercase">
                    {{ log.model_type }}{{ log.model_id ? ` #${log.model_id}` : '' }}
                  </span>
                </div>
                <span class="text-[10px] font-black uppercase text-gray-400 flex-shrink-0">
                  {{ formatDate(log.created_at) }}
                </span>
              </div>
              <div class="flex flex-wrap gap-3 mt-1.5">
                <span v-if="log.ip_address" class="text-xs font-mono text-gray-400 bg-gray-50 dark:bg-gray-900 px-2.5 py-1 rounded-lg">
                  {{ log.ip_address }}
                </span>
                <span v-if="log.payload && Object.keys(log.payload).length" class="text-xs font-mono text-gray-400 bg-gray-50 dark:bg-gray-900 px-2.5 py-1 rounded-lg truncate max-w-xs">
                  {{ JSON.stringify(log.payload) }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <div v-if="lastPage > 1" class="flex items-center justify-between px-6 py-4 border-t border-gray-100 dark:border-gray-800">
          <span class="text-[10px] font-black uppercase text-gray-400">
            Page {{ currentPage }} of {{ lastPage }} &middot; {{ total }} entries
          </span>
          <div class="flex gap-2">
            <BaseButton
                variant="ghost"
                size="sm"
                class="rounded-xl"
                :disabled="currentPage <= 1"
                @click="fetchLogs(currentPage - 1)"
            >
              <ChevronLeft class="w-4 h-4" />
            </BaseButton>
            <BaseButton
                variant="ghost"
                size="sm"
                class="rounded-xl"
                :disabled="currentPage >= lastPage"
                @click="fetchLogs(currentPage + 1)"
            >
              <ChevronRight class="w-4 h-4" />
            </BaseButton>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { Terminal, Loader2, ChevronLeft, ChevronRight } from 'lucide-vue-next';
import { API } from '@/api';
import type { LogEntry } from '@/api/logs.service';
import BaseButton from '@/components/UI/BaseButton.vue';

const loading = ref(true);
const logs = ref<LogEntry[]>([]);
const currentPage = ref(1);
const lastPage = ref(1);
const total = ref(0);

const filters = reactive({ action: '', date_from: '', date_to: '' });

const formatDate = (dateStr: string) =>
    new Date(dateStr).toLocaleString('pl-PL', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });

const fetchLogs = async (page = 1) => {
  loading.value = true;
  try {
    const params = {
      page,
      per_page: 20,
      ...(filters.action ? { action: filters.action } : {}),
      ...(filters.date_from ? { date_from: filters.date_from } : {}),
      ...(filters.date_to ? { date_to: filters.date_to } : {}),
    };
    const { data } = await API.logs.index(params);
    logs.value = data.data;
    currentPage.value = data.current_page;
    lastPage.value = data.last_page;
    total.value = data.total;
  } finally {
    loading.value = false;
  }
};

const resetFilters = () => {
  filters.action = '';
  filters.date_from = '';
  filters.date_to = '';
  fetchLogs(1);
};

onMounted(() => fetchLogs(1));
</script>
