<template>
  <header class="bg-restall-light/80 dark:bg-restall-dark/80 backdrop-blur-md border-b border-gray-200 dark:border-gray-800 sticky top-0 z-50 transition-colors duration-300">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-20 relative">

        <div class="flex items-center">
          <router-link to="/" class="text-2xl font-black text-restall-green tracking-tighter flex items-center space-x-2 z-50" @click="closeMobileMenu">
            <UtensilsCrossed class="w-8 h-8" />
            <span class="hidden xs:block text-restall-dark dark:text-restall-light">RestAll</span>
          </router-link>
        </div>

        <nav class="hidden md:flex items-center space-x-8 text-sm font-bold text-gray-600 dark:text-gray-400">
          <router-link v-for="link in publicLinks" :key="link.path" :to="link.path"
                       class="hover:text-restall-gold transition-colors"
                       active-class="text-restall-green dark:text-restall-gold">
            {{ t(link.label) }}
          </router-link>
        </nav>

        <div class="flex items-center space-x-2">
          <BaseButton variant="ghost" size="sm" @click="toggleTheme" class="rounded-xl px-3">
            <Sun v-if="isDark" class="w-5 h-5 text-restall-gold" />
            <Moon v-else class="w-5 h-5 text-gray-500" />
          </BaseButton>

          <div class="relative group hidden sm:flex items-center h-full">
            <BaseButton variant="ghost" size="sm" class="rounded-xl px-3">
              <Languages class="w-5 h-5 text-gray-500 dark:text-gray-400" />
              <span class="ml-1.5 text-xs font-black uppercase text-restall-dark dark:text-restall-light">{{ locale }}</span>
            </BaseButton>

            <div class="absolute top-full right-0 mt-2 w-32 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl shadow-xl py-2 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-50">
              <button
                  v-for="lang in ['en', 'pl']"
                  :key="lang"
                  @click="locale = lang"
                  class="w-full text-left px-4 py-3 text-xs font-black uppercase text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700 hover:text-restall-green transition-colors"
              >
                {{ lang }}
              </button>
            </div>
          </div>

          <div class="h-6 w-px bg-gray-200 dark:bg-gray-800 mx-2 hidden md:block"></div>

          <template v-if="!authStore.isAuth">
            <div class="hidden md:flex items-center space-x-3">
              <BaseButton to="/login" variant="ghost" size="sm">{{ t('auth.to_login') }}</BaseButton>
              <BaseButton to="/register" variant="primary" size="sm">{{ t('auth.to_register') }}</BaseButton>
            </div>
          </template>

          <template v-else>
            <div class="relative group hidden md:flex items-center h-full">
              <button class="flex items-center space-x-3 p-1.5 pl-4 bg-white dark:bg-gray-800 rounded-2xl border border-gray-100 dark:border-gray-700 shadow-sm transition-all hover:border-restall-green/30">
                <span class="text-xs font-black text-restall-dark dark:text-restall-light">{{ authStore.user?.name }}</span>
                <div class="w-8 h-8 rounded-xl bg-restall-green flex items-center justify-center text-restall-light font-black text-xs uppercase">
                  {{ authStore.user?.name?.[0] }}
                </div>
              </button>

              <div class="absolute top-full right-0 mt-2 w-56 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl shadow-xl py-2 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-50">
                <router-link v-for="action in userActions" :key="action.path" :to="action.path"
                             class="flex items-center px-4 py-3 text-sm font-bold text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700 hover:text-restall-green transition-colors">
                  <component :is="action.icon" class="w-4 h-4 mr-3" />
                  {{ t(action.label) }}
                </router-link>
                <hr class="my-2 border-gray-100 dark:border-gray-700" />
                <button @click="handleLogout" class="w-full flex items-center px-4 py-3 text-sm font-bold text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10 transition-colors">
                  <LogOut class="w-4 h-4 mr-3" />
                  {{ t('admin.logout') }}
                </button>
              </div>
            </div>
          </template>

          <button @click="toggleMobileMenu" class="md:hidden p-2 text-restall-dark dark:text-restall-light z-50">
            <X v-if="isMobileMenuOpen" class="w-6 h-6" />
            <Menu v-else class="w-6 h-6" />
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { UtensilsCrossed, Menu, X, Sun, Moon, LogOut, Languages } from 'lucide-vue-next';
import { useTheme } from '@/composables/useTheme';
import { useAuthStore } from '@/stores/auth.store';
import { useNavigation } from '@/composables/useNavigation';
import BaseButton from './BaseButton.vue';

const { isDark, toggleTheme } = useTheme();
const authStore = useAuthStore();
const {
  publicLinks, userActions, t, locale,
  isMobileMenuOpen, toggleMobileMenu, closeMobileMenu
} = useNavigation();

const handleLogout = async () => {
  closeMobileMenu();
  await authStore.logout();
  window.location.href = '/';
};
</script>