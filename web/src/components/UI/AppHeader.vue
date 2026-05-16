<template>
  <header class="bg-restall-light/80 dark:bg-restall-dark/80 backdrop-blur-md border-b border-gray-200 dark:border-gray-800 sticky top-0 z-50 transition-colors duration-300">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-20 relative">

        <div class="flex items-center">
          <router-link :to="{ name: 'Home' }" class="text-2xl font-black text-restall-green tracking-tighter flex items-center space-x-2 z-50" @click="closeMobileMenu">
            <UtensilsCrossed class="w-8 h-8" />
            <span class="hidden xs:block text-restall-dark dark:text-restall-light">RestAll</span>
          </router-link>
        </div>

        <nav class="hidden md:flex items-center space-x-8 text-sm font-bold text-gray-600 dark:text-gray-400">
          <router-link
              v-for="link in publicLinks"
              :key="link.name"
              :to="{ name: link.name }"
              class="hover:text-restall-gold transition-colors"
              active-class="text-restall-green dark:text-restall-gold"
          >
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
              <BaseButton :to="{ name: 'Login' }" variant="ghost" size="sm">{{ t('auth.to_login') }}</BaseButton>
              <BaseButton :to="{ name: 'Register' }" variant="primary" size="sm">{{ t('auth.to_register') }}</BaseButton>
            </div>
          </template>

          <template v-else>
            <div class="relative group flex items-center h-full">
              <button class="flex items-center space-x-3 p-1.5 pl-4 bg-white dark:bg-gray-800 rounded-2xl border border-gray-100 dark:border-gray-700 shadow-sm transition-all group-hover:border-restall-green/40">
                <div class="flex flex-col items-end hidden sm:block">
                  <span class="text-[10px] uppercase font-black text-gray-400 leading-none mb-1">
                    {{ t(`role.${authStore.userRole || 'guest'}`) }}
                  </span>
                  <p class="text-xs font-black text-restall-dark dark:text-restall-light leading-none">
                    {{ authStore.user?.name }}
                  </p>
                </div>
                <div class="w-9 h-9 rounded-xl bg-restall-green flex items-center justify-center text-restall-light font-black text-sm uppercase shadow-sm">
                  {{ userInitials }}
                </div>
              </button>

              <div class="absolute top-full right-0 mt-2 w-56 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl shadow-xl py-2 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-50 transform origin-top-right">
                <div class="px-4 py-3 border-b border-gray-50 dark:border-gray-700/50 mb-1">
                  <p class="text-[10px] font-black uppercase text-gray-400">{{ t('auth.logged_in_as') }}</p>
                  <p class="text-xs font-bold text-restall-dark dark:text-restall-light truncate">{{ authStore.user?.email }}</p>
                </div>
                <router-link
                    v-for="action in userActions"
                    :key="action.name"
                    :to="{ name: action.name }"
                    class="flex items-center px-4 py-2.5 text-sm font-bold text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700 hover:text-restall-green transition-colors"
                >
                  <component :is="action.icon" class="w-4 h-4 mr-3 opacity-70" />
                  {{ t(action.label) }}
                </router-link>
                <hr class="my-1.5 border-gray-100 dark:border-gray-700" />
                <button
                    @click="onLogout"
                    class="w-full flex items-center px-4 py-2.5 text-sm font-bold text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10 transition-colors"
                >
                  <LogOut class="w-4 h-4 mr-3" />
                  {{ t('admin.logout') }}
                </button>
              </div>
            </div>
          </template>

          <button @click="toggleMobileMenu" class="md:hidden p-2 text-restall-dark dark:text-restall-light z-50 ml-2">
            <X v-if="isMobileMenuOpen" class="w-6 h-6" />
            <Menu v-else class="w-6 h-6" />
          </button>
        </div>
      </div>
    </div>
  </header>

  <Teleport to="body">
    <Transition
        enter-active-class="transition duration-200 ease-out"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition duration-150 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
    >
      <div v-if="isMobileMenuOpen" class="md:hidden fixed inset-0 z-40 bg-restall-light dark:bg-restall-dark flex flex-col overflow-y-auto">
        <div class="flex-1 px-6 pt-24 pb-10 flex flex-col">

          <nav class="flex flex-col">
            <router-link
                v-for="link in publicLinks"
                :key="link.name"
                :to="{ name: link.name }"
                @click="closeMobileMenu"
                class="py-4 text-4xl font-black uppercase tracking-tighter text-restall-dark dark:text-restall-light border-b border-gray-100 dark:border-gray-800"
                active-class="text-restall-gold"
            >
              {{ t(link.label) }}
            </router-link>
          </nav>

          <div class="mt-10">
            <template v-if="!authStore.isAuth">
              <div class="flex flex-col gap-3">
                <BaseButton :to="{ name: 'Login' }" variant="primary" size="lg" class="w-full rounded-2xl" @click="closeMobileMenu">
                  {{ t('auth.to_login') }}
                </BaseButton>
                <BaseButton :to="{ name: 'Register' }" variant="ghost" size="lg" class="w-full rounded-2xl border border-gray-200 dark:border-gray-700" @click="closeMobileMenu">
                  {{ t('auth.to_register') }}
                </BaseButton>
              </div>
            </template>
            <template v-else>
              <div class="flex items-center gap-3 p-4 bg-gray-50 dark:bg-gray-900 rounded-2xl mb-4">
                <div class="w-10 h-10 rounded-xl bg-restall-green flex items-center justify-center text-restall-light font-black text-sm uppercase flex-shrink-0">
                  {{ userInitials }}
                </div>
                <div class="min-w-0">
                  <p class="font-black text-sm text-restall-dark dark:text-restall-light truncate">{{ authStore.user?.name }}</p>
                  <p class="text-xs text-gray-400 font-medium truncate">{{ authStore.user?.email }}</p>
                </div>
              </div>
              <div class="flex flex-col gap-1">
                <router-link
                    v-for="action in userActions"
                    :key="action.name"
                    :to="{ name: action.name }"
                    @click="closeMobileMenu"
                    class="flex items-center px-4 py-3 rounded-2xl font-bold text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                >
                  <component :is="action.icon" class="w-5 h-5 mr-3 opacity-60" />
                  {{ t(action.label) }}
                </router-link>
                <button @click="onLogout" class="flex items-center px-4 py-3 rounded-2xl font-bold text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10 transition-colors">
                  <LogOut class="w-5 h-5 mr-3" />
                  {{ t('admin.logout') }}
                </button>
              </div>
            </template>
          </div>

          <div class="mt-auto pt-10 flex items-center gap-3">
            <button
                v-for="lang in ['en', 'pl']"
                :key="lang"
                @click="locale = lang; closeMobileMenu()"
                :class="['px-4 py-2 rounded-xl text-xs font-black uppercase tracking-wider transition-colors', locale === lang ? 'bg-restall-dark text-white dark:bg-white dark:text-restall-dark' : 'text-gray-400 hover:text-gray-600 dark:hover:text-gray-300']"
            >{{ lang }}</button>
            <button @click="toggleTheme" class="ml-auto p-2 rounded-xl hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
              <Sun v-if="isDark" class="w-5 h-5 text-restall-gold" />
              <Moon v-else class="w-5 h-5 text-gray-500" />
            </button>
          </div>

        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { UtensilsCrossed, Menu, X, Sun, Moon, LogOut, Languages } from 'lucide-vue-next';
import { useTheme } from '@/composables/useTheme';
import { useAuthStore } from '@/stores/auth.store';
import { useNavigation } from '@/composables/useNavigation';
import BaseButton from './BaseButton.vue';

const router = useRouter();
const authStore = useAuthStore();
const { isDark, toggleTheme } = useTheme();
const {
  publicLinks, userActions, t, locale,
  isMobileMenuOpen, toggleMobileMenu, closeMobileMenu
} = useNavigation();

const userInitials = computed(() => authStore.user?.name?.charAt(0) || '?');

const onLogout = async () => {
  try {
    closeMobileMenu();
    await authStore.logout();
    await router.push({ name: 'Home' });
  } catch (error) {
    console.error('Logout operation failed', error);
  }
};
</script>