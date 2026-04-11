<template>
  <div class="space-y-10 font-sans">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
          Staff Management
        </h1>
      </div>
      <BaseButton variant="primary">
        <Plus class="w-5 h-5 mr-2" />
        Add Member
      </BaseButton>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="i in 6" :key="i" class="bg-white dark:bg-gray-800 p-6 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700 flex flex-col items-center text-center">
        <div class="w-20 h-20 rounded-2xl bg-restall-green/10 flex items-center justify-center text-restall-green font-black text-2xl mb-4">
          C{{ i }}
        </div>
        <h3 class="font-bold text-xl text-restall-dark dark:text-restall-light">Chef Gordon</h3>
        <p class="text-gray-500 text-sm font-medium mt-1 mb-4">gordon@restall.com</p>
        <BaseBadge :variant="i % 2 === 0 ? 'gold' : 'dark'" class="mb-6">
          {{ i % 2 === 0 ? 'Kitchen Staff' : 'Manager' }}
        </BaseBadge>
        <div class="w-full grid grid-cols-2 gap-2 mt-auto">
          <BaseButton variant="outline" size="sm" class="rounded-xl">Edit</BaseButton>
          <BaseButton variant="ghost" size="sm" class="rounded-xl text-rose-500">Revoke</BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Plus } from 'lucide-vue-next';
import BaseButton from '@/components/UI/BaseButton.vue';
import BaseBadge from '@/components/UI/BaseBadge.vue';
</script>