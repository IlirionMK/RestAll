<template>
  <div class="flex h-screen bg-gray-50 dark:bg-gray-900 transition-colors duration-200">
    <aside class="w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 hidden md:flex flex-col">
      <div class="p-6">
        <div class="text-2xl font-black text-emerald-600 tracking-tighter">RestAll</div>
      </div>

      <nav class="flex-1 px-4 space-y-1">
        <router-link v-for="item in menuItems" :key="item.path" :to="item.path"
                     class="flex items-center px-4 py-3 text-sm font-medium rounded-xl transition-colors"
                     :class="[$route.path === item.path ? 'bg-emerald-50 text-emerald-600 dark:bg-emerald-500/10' : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700/50']">
          <component :is="item.icon" class="w-5 h-5 mr-3" />
          {{ t(item.label) }}
        </router-link>
      </nav>

      <div class="p-4 border-t border-gray-200 dark:border-gray-700">
        <button @click="handleLogout" class="flex items-center w-full px-4 py-3 text-sm font-medium text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10 rounded-xl transition-colors">
          <LogOut class="w-5 h-5 mr-3" />
          {{ t('admin.logout') }}
        </button>
      </div>
    </aside>

    <div class="flex-1 flex flex-col overflow-hidden">
      <header class="h-16 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between px-8">
        <h2 class="text-lg font-bold text-gray-800 dark:text-white">{{ currentRouteName }}</h2>

        <div class="flex items-center space-x-4">
          <button @click="toggleTheme" class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-500 dark:text-gray-400">
            <Sun v-if="isDark" class="w-5 h-5" />
            <Moon v-else class="w-5 h-5" />
          </button>
          <div class="h-8 w-8 rounded-full bg-emerald-500 flex items-center justify-center text-white font-bold text-xs">
            AD
          </div>
        </div>
      </header>

      <main class="flex-1 overflow-y-auto p-8">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { useTheme } from '@/composables/useTheme';
import { useAuthStore } from '@/stores/auth.store';
import {
  LayoutDashboard, UtensilsCrossed, Users, ClipboardList, LogOut, Sun, Moon
} from 'lucide-vue-next';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const { isDark, toggleTheme } = useTheme();
const authStore = useAuthStore();

const menuItems = [
  { path: '/admin', label: 'admin.dashboard', icon: LayoutDashboard },
  { path: '/admin/menu', label: 'admin.menu', icon: UtensilsCrossed },
  { path: '/admin/staff', label: 'admin.staff', icon: Users },
  { path: '/admin/logs', label: 'Logs', icon: ClipboardList },
];

const currentRouteName = computed(() => {
  const current = menuItems.find(item => item.path === route.path);
  return current ? t(current.label) : 'Dashboard';
});

const handleLogout = async () => {
  await authStore.logout();
  router.push('/login');
};
</script>