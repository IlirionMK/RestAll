<template>
  <header class="bg-white/80 dark:bg-gray-900/80 backdrop-blur-md border-b border-gray-200 dark:border-gray-800 sticky top-0 z-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-16">

        <div class="flex items-center">
          <router-link to="/" class="text-2xl font-black text-emerald-600 tracking-tighter flex items-center space-x-2 z-50" @click="closeMobileMenu">
            <UtensilsCrossed class="w-8 h-8" />
            <span class="hidden xs:block">RestAll</span>
          </router-link>
        </div>

        <nav class="hidden md:flex items-center space-x-8 text-sm font-bold text-gray-600 dark:text-gray-400">
          <router-link v-for="link in publicLinks" :key="link.path" :to="link.path"
                       class="hover:text-emerald-500 transition-colors"
                       active-class="text-emerald-600">
            {{ t(link.label) }}
          </router-link>
        </nav>

        <div class="flex items-center space-x-1 sm:space-x-3">
          <BaseButton variant="ghost" size="sm" @click="toggleTheme" class="rounded-full">
            <Sun v-if="isDark" class="w-5 h-5" />
            <Moon v-else class="w-5 h-5" />
          </BaseButton>

          <div class="h-6 w-px bg-gray-200 dark:bg-gray-800 hidden sm:block"></div>

          <template v-if="!authStore.isAuth">
            <div class="hidden md:flex items-center space-x-2">
              <BaseButton to="/login" variant="ghost" size="sm">{{ t('auth.to_login') }}</BaseButton>
              <BaseButton to="/register" variant="primary" size="sm">{{ t('auth.to_register') }}</BaseButton>
            </div>
          </template>

          <template v-else>
            <div class="relative group hidden md:block">
              <button class="flex items-center space-x-2 p-1 pl-3 bg-gray-50 dark:bg-gray-800 rounded-xl border border-transparent group-hover:border-emerald-500/30 transition-all">
                <span class="text-xs font-black dark:text-white">{{ authStore.user?.name }}</span>
                <div class="w-8 h-8 rounded-lg bg-emerald-500 flex items-center justify-center text-white font-black text-[10px]">
                  {{ authStore.user?.name?.[0]?.toUpperCase() }}
                </div>
              </button>
              <div class="absolute right-0 mt-2 w-56 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl shadow-2xl py-2 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all">
                <router-link v-for="action in userActions" :key="action.path" :to="action.path"
                             class="flex items-center px-4 py-2 text-sm font-bold text-gray-600 dark:text-gray-300 hover:bg-emerald-50 dark:hover:bg-emerald-500/10 hover:text-emerald-600">
                  <component :is="action.icon" class="w-4 h-4 mr-3" />
                  {{ t(action.label) }}
                </router-link>
                <hr class="my-2 border-gray-100 dark:border-gray-700" />
                <button @click="handleLogout" class="w-full flex items-center px-4 py-2 text-sm font-bold text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10">
                  <LogOut class="w-4 h-4 mr-3" />
                  {{ t('admin.logout') }}
                </button>
              </div>
            </div>
          </template>

          <button @click="toggleMobileMenu" class="md:hidden p-2 text-gray-600 dark:text-gray-400 z-50">
            <X v-if="isMobileMenuOpen" class="w-6 h-6" />
            <Menu v-else class="w-6 h-6" />
          </button>
        </div>
      </div>
    </div>

    <transition
        enter-active-class="transition duration-300 ease-out"
        enter-from-class="opacity-0 translate-x-full"
        enter-to-class="opacity-100 translate-x-0"
        leave-active-class="transition duration-200 ease-in"
        leave-from-class="opacity-100 translate-x-0"
        leave-to-class="opacity-0 translate-x-full"
    >
      <div v-if="isMobileMenuOpen" class="fixed inset-0 z-40 bg-white dark:bg-gray-950 md:hidden">
        <div class="flex flex-col h-full p-6 pt-24">
          <nav class="flex flex-col space-y-6 mb-12">
            <router-link v-for="link in publicLinks" :key="link.path" :to="link.path"
                         class="text-3xl font-black text-gray-900 dark:text-white" @click="closeMobileMenu">
              {{ t(link.label) }}
            </router-link>
          </nav>

          <div class="mt-auto space-y-4">
            <template v-if="authStore.isAuth">
              <router-link v-for="action in userActions" :key="action.path" :to="action.path"
                           class="flex items-center p-4 bg-gray-50 dark:bg-gray-900 rounded-2xl text-lg font-bold dark:text-white" @click="closeMobileMenu">
                <component :is="action.icon" class="w-6 h-6 mr-4 text-emerald-500" />
                {{ t(action.label) }}
              </router-link>
              <BaseButton variant="danger" size="lg" class="w-full" @click="handleLogout">{{ t('admin.logout') }}</BaseButton>
            </template>
            <template v-else>
              <BaseButton to="/login" variant="outline" size="lg" class="w-full" @click="closeMobileMenu">{{ t('auth.to_login') }}</BaseButton>
              <BaseButton to="/register" variant="primary" size="lg" class="w-full" @click="closeMobileMenu">{{ t('auth.to_register') }}</BaseButton>
            </template>
          </div>
        </div>
      </div>
    </transition>
  </header>
</template>

<script setup lang="ts">
import { UtensilsCrossed, Menu, X, Sun, Moon, LogOut } from 'lucide-vue-next';
import { useTheme } from '@/composables/useTheme';
import { useAuthStore } from '@/stores/auth.store';
import { useNavigation } from '@/composables/useNavigation';
import BaseButton from './BaseButton.vue';

const { isDark, toggleTheme } = useTheme();
const authStore = useAuthStore();
const {
  publicLinks, userActions, t,
  isMobileMenuOpen, toggleMobileMenu, closeMobileMenu
} = useNavigation();

const handleLogout = async () => {
  closeMobileMenu();
  await authStore.logout();
  window.location.href = '/';
};
</script>