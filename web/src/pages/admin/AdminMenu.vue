<template>
  <div class="space-y-10 font-sans">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
          Menu Management
        </h1>
      </div>
      <BaseButton variant="primary">
        <Plus class="w-5 h-5 mr-2" />
        Add Item
      </BaseButton>
    </div>

    <div class="bg-white dark:bg-gray-800 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
      <div class="p-6 border-b border-gray-100 dark:border-gray-700 flex gap-4">
        <input
            type="text"
            placeholder="Search menu items..."
            class="flex-1 px-4 py-3 bg-gray-50 dark:bg-gray-900 border-none rounded-2xl focus:ring-2 focus:ring-restall-green outline-none text-restall-dark dark:text-restall-light font-bold"
        >
        <select class="px-4 py-3 bg-gray-50 dark:bg-gray-900 border-none rounded-2xl focus:ring-2 focus:ring-restall-green outline-none font-bold text-gray-500 cursor-pointer">
          <option>All Categories</option>
          <option>Starters</option>
          <option>Main Courses</option>
          <option>Desserts</option>
        </select>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
          <tr class="bg-gray-50 dark:bg-gray-900/50">
            <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Item</th>
            <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Category</th>
            <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Price</th>
            <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Status</th>
            <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400 text-right">Actions</th>
          </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
          <tr v-for="i in 5" :key="i" class="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
            <td class="px-6 py-4">
              <p class="font-bold text-restall-dark dark:text-restall-light">A5 Wagyu Beef Filet</p>
              <p class="text-xs text-gray-500 mt-1">Premium cut with truffle puree</p>
            </td>
            <td class="px-6 py-4 font-bold text-gray-600 dark:text-gray-300">Main Course</td>
            <td class="px-6 py-4 font-black text-restall-gold">$85.00</td>
            <td class="px-6 py-4">
              <BaseBadge variant="green">Available</BaseBadge>
            </td>
            <td class="px-6 py-4 flex justify-end gap-2">
              <BaseButton variant="ghost" size="sm" class="px-3 rounded-xl"><Edit class="w-4 h-4" /></BaseButton>
              <BaseButton variant="ghost" size="sm" class="px-3 rounded-xl text-rose-500 hover:text-rose-600 dark:text-rose-500"><Trash2 class="w-4 h-4" /></BaseButton>
            </td>
          </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Plus, Edit, Trash2 } from 'lucide-vue-next';
import BaseButton from '@/components/UI/BaseButton.vue';
import BaseBadge from '@/components/UI/BaseBadge.vue';
</script>