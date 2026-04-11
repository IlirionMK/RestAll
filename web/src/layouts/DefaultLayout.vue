<template>
  <div class="min-h-screen flex flex-col bg-restall-light dark:bg-restall-dark text-restall-dark dark:text-restall-light font-sans antialiased selection:bg-restall-green selection:text-restall-light transition-colors duration-300">
    <AppHeader />

    <main class="flex-1 flex flex-col w-full relative">
      <router-view v-slot="{ Component }">
        <transition
            enter-active-class="transition duration-300 ease-out"
            enter-from-class="opacity-0 translate-y-4"
            enter-to-class="opacity-100 translate-y-0"
            leave-active-class="transition duration-200 ease-in"
            leave-from-class="opacity-100 translate-y-0"
            leave-to-class="opacity-0 -translate-y-4"
            mode="out-in"
        >
          <component :is="Component" />
        </transition>
      </router-view>
    </main>

    <AppFooter />
  </div>
</template>

<script setup lang="ts">
import AppHeader from '@/components/UI/AppHeader.vue';
import AppFooter from '@/components/UI/AppFooter.vue';
</script>