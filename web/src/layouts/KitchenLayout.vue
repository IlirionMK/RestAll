<template>
  <div class="min-h-screen bg-gray-100 dark:bg-gray-900 transition-colors duration-200">
    <header class="h-16 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between px-6 shadow-sm">
      <h1 class="text-xl font-bold text-gray-900 dark:text-white">KDS</h1>
      <button @click="handleLogout" class="text-sm bg-rose-500 text-white px-4 py-1.5 rounded hover:bg-rose-600 transition">
        Logout
      </button>
    </header>
    <main class="p-6">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth.store';

const router = useRouter();
const authStore = useAuthStore();

const handleLogout = () => {
  authStore.logout();
  router.push('/login');
};
</script>