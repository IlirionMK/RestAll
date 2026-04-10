<template>
  <div class="max-w-4xl mx-auto space-y-12">
    <div class="flex justify-between items-center">
      <h1 class="text-3xl font-black">{{ t('nav.reviews') }}</h1>
      <BaseButton variant="outline" size="sm">
        <Plus class="w-4 h-4 mr-2" />
        {{ t('reviews.write') }}
      </BaseButton>
    </div>

    <div class="grid gap-6">
      <div v-for="i in 3" :key="i" class="bg-white dark:bg-gray-800 p-6 rounded-3xl border border-gray-100 dark:border-gray-700">
        <div class="flex justify-between items-start mb-4">
          <div class="flex items-center space-x-3">
            <div class="w-10 h-10 rounded-full bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center font-black text-emerald-600">
              U{{ i }}
            </div>
            <div>
              <p class="font-bold">User Name</p>
              <div class="flex text-amber-400">
                <Star v-for="s in 5" :key="s" class="w-3 h-3 fill-current" />
              </div>
            </div>
          </div>
          <span class="text-xs text-gray-400 font-bold">12.04.2026</span>
        </div>
        <p class="text-gray-600 dark:text-gray-400 text-sm leading-relaxed">
          Great food and amazing atmosphere! The service was top-notch.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { Star, Plus } from 'lucide-vue-next';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
</script>