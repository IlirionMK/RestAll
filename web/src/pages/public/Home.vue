<template>
  <div class="space-y-24 pb-20 overflow-x-hidden">
    <section class="relative h-[85vh] flex items-center justify-center overflow-hidden rounded-[3rem] mx-4 shadow-2xl mt-4">
      <div class="absolute inset-0 bg-gradient-to-b from-restall-dark/70 via-restall-dark/40 to-restall-dark/80 z-10"></div>

      <img
          src="https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?auto=format&fit=crop&q=80&w=2070"
          class="absolute inset-0 w-full h-full object-cover scale-105 animate-slow-zoom"
          alt="Hero"
      />

      <div class="relative z-20 text-center max-w-4xl px-6">
        <span class="inline-block py-1 px-4 rounded-full bg-restall-gold/20 text-restall-gold text-xs font-black uppercase tracking-widest mb-6 animate-fade-in">
          {{ t('public.hero_badge') }}
        </span>

        <h1 class="text-5xl md:text-8xl font-black text-restall-light tracking-tighter mb-6 uppercase leading-[0.9] animate-fade-in-up">
          {{ t('public.hero_title') }}
        </h1>

        <p class="text-lg md:text-2xl text-restall-light/80 font-medium mb-10 max-w-2xl mx-auto leading-relaxed animate-fade-in-up">
          {{ t('public.hero_subtitle') }}
        </p>

        <div class="flex flex-col sm:flex-row items-center justify-center gap-6 animate-fade-in-up">
          <BaseButton
              :to="{ name: 'BookingForm' }"
              size="lg"
              class="w-full sm:w-auto px-10 rounded-full py-5 text-lg no-underline shadow-xl shadow-restall-gold/20"
          >
            {{ t('public.book_btn') }}
          </BaseButton>

          <BaseButton
              :to="{ name: 'PublicMenu' }"
              variant="outline"
              size="lg"
              class="w-full sm:w-auto px-10 rounded-full py-5 text-lg no-underline"
          >
            {{ t('nav.menu') }}
          </BaseButton>
        </div>
      </div>
    </section>

    <section class="max-w-7xl mx-auto px-6">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <div v-for="i in 3" :key="i"
             class="group p-10 rounded-[2.5rem] bg-white dark:bg-gray-800/40 border border-gray-100 dark:border-gray-700 shadow-xl shadow-gray-200/50 dark:shadow-none transition-all duration-500 hover:-translate-y-3 hover:shadow-2xl">
          <div class="w-16 h-16 bg-restall-green/10 rounded-2xl flex items-center justify-center mb-8 text-restall-green transition-transform group-hover:rotate-12 duration-500">
            <component :is="[Utensils, Star, Clock][i-1]" class="w-8 h-8" />
          </div>
          <h3 class="text-2xl font-black text-restall-dark dark:text-restall-light mb-4 uppercase tracking-tight">
            {{ t(`public.feature_${i}_title`) }}
          </h3>
          <p class="text-gray-500 dark:text-gray-400 font-medium leading-relaxed">
            {{ t(`public.feature_${i}_desc`) }}
          </p>
        </div>
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

<style scoped>
@keyframes slow-zoom { 0% { transform: scale(1); } 100% { transform: scale(1.15); } }
@keyframes fade-in-up { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
@keyframes fade-in { from { opacity: 0; } to { opacity: 1; } }
.animate-slow-zoom { animation: slow-zoom 20s infinite alternate ease-in-out; }
.animate-fade-in { animation: fade-in 1.2s forwards; }
.animate-fade-in-up { opacity: 0; animation: fade-in-up 0.8s forwards; animation-delay: 0.3s; }
</style>