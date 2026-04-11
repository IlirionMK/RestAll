<template>
  <div class="max-w-4xl mx-auto px-6 py-12 md:py-20 font-sans space-y-12">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-6">
      <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
        {{ t('nav.reviews') }}
      </h1>
      <BaseButton variant="outline" size="md">
        <Plus class="w-5 h-5 mr-2" />
        {{ t('reviews.write') }}
      </BaseButton>
    </div>

    <div class="grid gap-6">
      <div v-for="i in 3" :key="i" class="bg-white dark:bg-gray-800 p-8 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700 transition-colors duration-300">
        <div class="flex justify-between items-start mb-6">
          <div class="flex items-center space-x-4">
            <div class="w-12 h-12 rounded-2xl bg-restall-green/10 flex items-center justify-center font-black text-restall-green text-lg">
              U{{ i }}
            </div>
            <div>
              <p class="font-bold text-restall-dark dark:text-restall-light">User Name</p>
              <div class="flex text-restall-gold mt-1 gap-0.5">
                <Star v-for="s in 5" :key="s" class="w-4 h-4 fill-current" />
              </div>
            </div>
          </div>
          <span class="text-[10px] font-black uppercase tracking-widest text-gray-400 dark:text-gray-500 mt-2">12.04.2026</span>
        </div>
        <p class="text-gray-600 dark:text-gray-300 leading-relaxed font-medium">
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