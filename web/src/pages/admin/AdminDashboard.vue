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

    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
      <BaseStatCard title="Today's Revenue" value="$4,250" :trend="12.5">
        <template #icon><DollarSign class="w-6 h-6" /></template>
      </BaseStatCard>
      <BaseStatCard title="Active Bookings" value="24" :trend="5.2">
        <template #icon><Calendar class="w-6 h-6" /></template>
      </BaseStatCard>
      <BaseStatCard title="Pending Orders" value="12" :trend="-2.4">
        <template #icon><ShoppingBag class="w-6 h-6" /></template>
      </BaseStatCard>
      <BaseStatCard title="Total Guests" value="148" :trend="18.1">
        <template #icon><Users class="w-6 h-6" /></template>
      </BaseStatCard>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div class="lg:col-span-2 bg-white dark:bg-gray-800 p-8 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700">
        <h2 class="text-2xl font-black text-restall-dark dark:text-restall-light mb-6">Recent Activity</h2>
        <div class="py-12 text-center text-gray-400 font-medium border-2 border-dashed border-gray-100 dark:border-gray-700 rounded-2xl">
          Chart visualization will be rendered here
        </div>
      </div>

      <div class="bg-white dark:bg-gray-800 p-8 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700">
        <h2 class="text-2xl font-black text-restall-dark dark:text-restall-light mb-6">Upcoming Reservations</h2>
        <div class="space-y-4">
          <div v-for="i in 4" :key="i" class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 rounded-2xl">
            <div>
              <p class="font-bold text-restall-dark dark:text-restall-light">Table #{{ i + 2 }}</p>
              <p class="text-xs text-gray-500 font-medium mt-1">19:30 • {{ i * 2 }} Guests</p>
            </div>
            <BaseBadge variant="gold">Pending</BaseBadge>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { DollarSign, Calendar, ShoppingBag, Users } from 'lucide-vue-next';
import BaseStatCard from '@/components/UI/BaseStatCard.vue';
import BaseBadge from '@/components/UI/BaseBadge.vue';
</script>