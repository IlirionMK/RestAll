<template>
  <div class="flex h-screen bg-gray-50 dark:bg-gray-950 overflow-hidden font-sans">
    <aside class="hidden lg:flex lg:flex-shrink-0">
      <div class="flex flex-col w-72 border-r border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
        <div class="flex items-center h-20 px-8 border-b border-gray-100 dark:border-gray-800">
          <router-link to="/admin" class="flex items-center space-x-3">
            <div class="p-2 bg-emerald-600 rounded-xl shadow-lg shadow-emerald-600/20">
              <UtensilsCrossed class="w-6 h-6 text-white" />
            </div>
            <span class="text-xl font-black tracking-tighter dark:text-white">AdminPanel</span>
          </router-link>
        </div>

        <nav class="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
          <router-link v-for="item in adminMenu" :key="item.path" :to="item.path"
                       class="flex items-center px-4 py-3 text-sm font-bold rounded-2xl transition-all group"
                       :class="[$route.path === item.path ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-600/20' : 'text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800']">
            <component :is="item.icon" class="w-5 h-5 mr-3" :class="$route.path === item.path ? 'text-white' : 'text-gray-400 group-hover:text-emerald-500'" />
            {{ t(item.label) }}
          </router-link>
        </nav>

        <div class="p-4 border-t border-gray-100 dark:border-gray-800">
          <BaseButton variant="danger" class="w-full !justify-start" @click="handleLogout">
            <LogOut class="w-5 h-5 mr-3" />
            {{ t('admin.logout') }}
          </BaseButton>
        </div>
      </div>
    </aside>

    <div class="flex flex-col flex-1 w-0 overflow-hidden">
      <header class="relative z-10 flex-shrink-0 flex h-20 bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800 px-8">
        <div class="flex-1 flex justify-between items-center">
          <h2 class="text-lg font-black dark:text-white uppercase tracking-widest text-gray-400">{{ $route.name }}</h2>
          <div class="flex items-center space-x-4">
            <BaseButton variant="ghost" size="sm" @click="toggleTheme">
              <Sun v-if="isDark" class="w-5 h-5" />
              <Moon v-else class="w-5 h-5" />
            </BaseButton>
            <div class="w-10 h-10 rounded-2xl bg-emerald-500 flex items-center justify-center text-white font-black">
              {{ authStore.user?.name?.[0] }}
            </div>
          </div>
        </div>
      </header>

      <main class="flex-1 relative overflow-y-auto focus:outline-none p-8">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import {
  UtensilsCrossed, LayoutDashboard, Utensils, Users,
  ClipboardList, LogOut, Sun, Moon
} from 'lucide-vue-next';
import { useTheme } from '@/composables/useTheme';
import { useAuthStore } from '@/stores/auth.store';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const router = useRouter();
const { isDark, toggleTheme } = useTheme();
const authStore = useAuthStore();

const adminMenu = [
  { path: '/admin', label: 'admin.dashboard', icon: LayoutDashboard },
  { path: '/admin/menu', label: 'admin.menu', icon: Utensils },
  { path: '/admin/staff', label: 'admin.staff', icon: Users },
  { path: '/admin/logs', label: 'admin.logs', icon: ClipboardList },
];

const handleLogout = async () => {
  await authStore.logout();
  router.push('/');
};
</script>