<template>
  <div class="h-screen flex flex-col bg-[#1F2937] text-[#FAF9F6] overflow-hidden font-sans antialiased selection:bg-[#14532D] selection:text-[#FAF9F6]">
    <header class="h-20 bg-[#1F2937] border-b border-gray-800/50 px-6 sm:px-8 flex justify-between items-center shadow-sm z-10">

      <div class="flex items-center space-x-6">
        <div class="flex items-center space-x-3">
          <div class="w-10 h-10 rounded-2xl bg-[#14532D] flex items-center justify-center shadow-lg shadow-[#14532D]/20">
            <ChefHat class="w-5 h-5 text-[#FAF9F6]" />
          </div>
          <h1 class="text-xl sm:text-2xl font-black tracking-tighter text-[#FAF9F6] uppercase">
            RestAll <span class="text-[#C29B40]">KDS</span>
          </h1>
        </div>

        <div class="hidden sm:flex space-x-4">
          <div class="px-4 py-1.5 bg-[#14532D] rounded-2xl text-[10px] font-black uppercase tracking-widest text-[#FAF9F6] shadow-sm">
            Live Tickets
          </div>
        </div>
      </div>

      <div class="flex items-center space-x-4 sm:space-x-6">
        <div class="text-right hidden sm:block">
          <p class="text-[10px] font-black uppercase text-[#C29B40] tracking-widest">Shift Leader</p>
          <p class="font-bold text-sm">{{ authStore.user?.name || 'Chef' }}</p>
        </div>

        <button @click="handleLogout" class="p-3 bg-gray-800/50 hover:bg-rose-500 text-gray-400 hover:text-white transition-all duration-300 rounded-2xl shadow-sm hover:shadow-rose-500/25 group">
          <LogOut class="w-5 h-5 group-hover:-translate-x-0.5 transition-transform" />
        </button>
      </div>

    </header>

    <main class="flex-1 overflow-x-auto p-4 sm:p-8 bg-[#1F2937]">
      <router-view v-slot="{ Component }">
        <transition
            enter-active-class="transition duration-300 ease-out"
            enter-from-class="opacity-0 scale-95"
            enter-to-class="opacity-100 scale-100"
            leave-active-class="transition duration-200 ease-in"
            leave-from-class="opacity-100 scale-100"
            leave-to-class="opacity-0 scale-95"
            mode="out-in"
        >
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import { LogOut, ChefHat } from 'lucide-vue-next';
import { useAuthStore } from '@/stores/auth.store';

const router = useRouter();
const authStore = useAuthStore();

const handleLogout = async () => {
  await authStore.logout();
  router.push('/');
};
</script>