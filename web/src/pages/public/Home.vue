<template>
  <div class="space-y-24 pb-20">
    <section class="relative h-[80vh] flex items-center justify-center overflow-hidden rounded-[3rem] mx-4 shadow-2xl">
      <div class="absolute inset-0 bg-[#1F2937]/60 z-10"></div>
      <img
          src="https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?auto=format&fit=crop&q=80&w=2070"
          class="absolute inset-0 w-full h-full object-cover scale-105"
          alt="Hero"
      />

      <div class="relative z-20 text-center max-w-3xl px-6">
        <h1 class="text-5xl md:text-7xl font-black text-[#FAF9F6] tracking-tighter mb-6">
          {{ t('public.hero_title') }}
        </h1>
        <p class="text-lg md:text-xl text-[#FAF9F6]/90 font-medium mb-10 leading-relaxed">
          {{ t('public.hero_subtitle') }}
        </p>
        <div class="flex flex-col sm:flex-row items-center justify-center gap-4">
          <BaseButton to="/bookings" variant="primary" size="lg" class="w-full sm:w-auto px-12 py-5 text-lg bg-[#C29B40] hover:bg-[#b08b39] text-[#FAF9F6] border-none shadow-lg shadow-[#C29B40]/30 transition-all rounded-2xl">
            {{ t('public.book_btn') }}
          </BaseButton>
          <BaseButton to="/menu" variant="secondary" size="lg" class="w-full sm:w-auto px-12 py-5 text-lg bg-white/10 backdrop-blur-md border border-white/20 text-[#FAF9F6] hover:bg-white/20 transition-all rounded-2xl">
            {{ t('nav.menu') }}
          </BaseButton>
        </div>
      </div>
    </section>

    <section class="max-w-7xl mx-auto px-6 grid grid-cols-1 md:grid-cols-3 gap-12 text-center">
      <div v-for="i in 3" :key="i" class="space-y-4">
        <div class="w-16 h-16 bg-[#14532D]/10 rounded-3xl flex items-center justify-center mx-auto transition-transform hover:scale-105 duration-300">
          <component :is="[Utensils, Star, Clock][i-1]" class="w-8 h-8 text-[#14532D]" />
        </div>
        <h3 class="text-xl font-black text-[#1F2937] dark:text-[#FAF9F6]">{{ t(`public.feature_${i}_title`) }}</h3>
        <p class="text-gray-500 dark:text-gray-400 text-sm leading-relaxed">
          {{ t(`public.feature_${i}_desc`) }}
        </p>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { Utensils, Star, Clock } from 'lucide-vue-next';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
</script>