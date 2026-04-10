<template>
  <footer class="bg-white dark:bg-gray-950 border-t border-gray-100 dark:border-gray-900 pt-16 pb-8">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-12 mb-12">

        <div class="space-y-4 col-span-1 sm:col-span-2 md:col-span-1">
          <router-link to="/" class="text-2xl font-black text-emerald-600 tracking-tighter flex items-center space-x-2">
            <UtensilsCrossed class="w-6 h-6" />
            <span>RestAll</span>
          </router-link>
          <p class="text-sm text-gray-500 dark:text-gray-400 leading-relaxed max-w-xs">
            A comprehensive ecosystem for modern restaurant management and exceptional guest experiences.
          </p>
        </div>

        <div v-for="section in footerSections" :key="section.title">
          <h4 class="text-xs font-black uppercase tracking-widest text-gray-900 dark:text-white mb-6">
            {{ t(section.title) }}
          </h4>
          <ul class="space-y-4 text-sm font-bold text-gray-500 dark:text-gray-400">
            <li v-for="link in section.links" :key="link.path">
              <router-link :to="link.path" class="hover:text-emerald-500 transition-colors">
                {{ t(link.label) }}
              </router-link>
            </li>
          </ul>
        </div>

        <div>
          <h4 class="text-xs font-black uppercase tracking-widest text-gray-900 dark:text-white mb-6">
            {{ t('footer.follow_us') }}
          </h4>
          <div class="flex space-x-3">
            <a v-for="(social, index) in socialLinks" :key="index" :href="social.href"
               class="p-2.5 bg-gray-50 dark:bg-gray-900 text-gray-400 rounded-xl hover:text-emerald-500 hover:scale-110 transition-all">
              <component :is="social.icon" class="w-5 h-5" />
            </a>
          </div>
        </div>
      </div>

      <div class="pt-8 border-t border-gray-50 dark:border-gray-900 flex flex-col md:flex-row justify-between items-center gap-4">
        <p class="text-[10px] font-black uppercase tracking-widest text-gray-400">
          © 2026 RestAll Ecosystem. All rights reserved.
        </p>
        <div class="flex space-x-6 text-[10px] font-black uppercase tracking-widest text-gray-400">
          <a href="#" class="hover:text-emerald-500">Privacy</a>
          <a href="#" class="hover:text-emerald-500">Terms</a>
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