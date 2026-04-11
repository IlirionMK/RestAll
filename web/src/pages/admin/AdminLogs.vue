<template>
  <div class="space-y-10 font-sans">
    <div>
      <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
        System Logs
      </h1>
    </div>

    <div class="bg-white dark:bg-gray-800 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden p-6">
      <div class="space-y-4">
        <div v-for="i in 8" :key="i" class="flex items-start gap-4 p-4 rounded-2xl hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors border border-transparent hover:border-gray-100 dark:hover:border-gray-800">
          <div class="w-10 h-10 rounded-xl flex-shrink-0 flex items-center justify-center bg-gray-100 dark:bg-gray-700">
            <Terminal class="w-5 h-5 text-gray-500" />
          </div>
          <div class="flex-1">
            <div class="flex items-center justify-between">
              <p class="font-bold text-restall-dark dark:text-restall-light">
                <span v-if="i % 3 === 0" class="text-rose-500 mr-2">[ERROR]</span>
                <span v-else-if="i % 2 === 0" class="text-restall-gold mr-2">[WARN]</span>
                <span v-else class="text-restall-green mr-2">[INFO]</span>
                User authentication {{ i % 3 === 0 ? 'failed' : 'successful' }}
              </p>
              <span class="text-[10px] font-black uppercase text-gray-400">10:4{{ i }}:22 AM</span>
            </div>
            <p class="text-sm text-gray-500 mt-1 font-mono bg-gray-50 dark:bg-gray-900 p-2 rounded-lg inline-block">
              IP: 192.168.1.{{ i * 15 }} | ID: req_8x9{{ i }}m2
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Terminal } from 'lucide-vue-next';
</script>