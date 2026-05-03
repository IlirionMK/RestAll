<template>
  <footer class="bg-transparent border-t border-gray-200 dark:border-gray-800 pt-16 pb-8 mt-auto transition-colors duration-300">
    <div class="max-w-7xl mx-auto px-6 lg:px-8">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-12 mb-12">
        <div class="col-span-1 md:col-span-2">
          <div class="flex items-center space-x-2 mb-6">
            <UtensilsCrossed class="w-8 h-8 text-restall-green" />
            <span class="text-2xl font-black text-restall-dark dark:text-restall-light tracking-tighter">RestAll</span>
          </div>
          <p class="text-gray-500 dark:text-gray-400 mb-6 max-w-sm">
            {{ t('public.hero_subtitle') }}
          </p>
        </div>

        <div v-for="section in footerSections" :key="section.title">
          <h4 class="font-black text-restall-dark dark:text-restall-light uppercase tracking-widest text-xs mb-6">
            {{ t(section.title) }}
          </h4>
          <ul class="space-y-4">
            <li v-for="link in section.links" :key="link.name">
              <router-link :to="{ name: link.name, params: link.params || {} }" class="text-gray-500 dark:text-gray-400 hover:text-restall-gold font-medium transition-colors">
                {{ t(link.label) }}
              </router-link>
            </li>
          </ul>
        </div>
      </div>

      <div class="pt-8 border-t border-gray-200 dark:border-gray-800 flex flex-col md:flex-row justify-between items-center gap-4">
        <p class="text-sm text-gray-500 dark:text-gray-400 font-medium">
          &copy; {{ new Date().getFullYear() }} RestAll. All rights reserved.
        </p>
        <div class="flex space-x-4">
          <a v-for="social in socialLinks" :key="social.href" :href="social.href" class="w-10 h-10 rounded-xl bg-white dark:bg-gray-800 shadow-sm flex items-center justify-center text-gray-500 hover:text-restall-green transition-colors">
            <component :is="social.icon" class="w-5 h-5" />
          </a>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { UtensilsCrossed } from 'lucide-vue-next';
import { useNavigation } from '@/composables/useNavigation';

const { footerSections, socialLinks, t } = useNavigation();
</script>