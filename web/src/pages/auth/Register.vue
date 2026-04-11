<template>
  <div class="max-w-md mx-auto w-full py-12 px-6 font-sans">
    <div class="bg-white dark:bg-gray-800 p-8 md:p-10 rounded-squircle shadow-2xl shadow-restall-green/5 border border-gray-100 dark:border-gray-700">
      <div class="text-center mb-10">
        <h1 class="text-3xl font-black text-restall-dark dark:text-restall-light uppercase tracking-tighter">
          {{ t('auth.register_title') }}
        </h1>
        <p class="mt-2 text-sm text-gray-500 dark:text-gray-400 font-medium">
          {{ t('auth.register_subtitle') }}
        </p>
      </div>

      <div v-if="errorMessage" class="mb-6 p-4 bg-rose-500/10 border border-rose-500/20 rounded-2xl flex items-start text-rose-500 text-sm font-bold transition-all">
        <AlertCircle class="w-5 h-5 mr-3 flex-shrink-0" />
        <p>{{ errorMessage }}</p>
      </div>

      <form @submit.prevent="handleRegister" class="space-y-6">
        <div class="space-y-2">
          <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 dark:text-gray-500 ml-1">
            {{ t('auth.name') }}
          </label>
          <div class="relative">
            <User class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
                v-model="form.name"
                type="text"
                required
                class="w-full pl-12 pr-4 py-4 bg-gray-50 dark:bg-gray-900 border border-gray-100 dark:border-gray-800 rounded-2xl focus:ring-2 focus:ring-restall-green outline-none transition-all text-restall-dark dark:text-restall-light font-bold"
            >
          </div>
        </div>

        <div class="space-y-2">
          <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 dark:text-gray-500 ml-1">
            {{ t('auth.email') }}
          </label>
          <div class="relative">
            <Mail class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
                v-model="form.email"
                type="email"
                required
                class="w-full pl-12 pr-4 py-4 bg-gray-50 dark:bg-gray-900 border border-gray-100 dark:border-gray-800 rounded-2xl focus:ring-2 focus:ring-restall-green outline-none transition-all text-restall-dark dark:text-restall-light font-bold"
            >
          </div>
        </div>

        <div class="space-y-2">
          <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 dark:text-gray-500 ml-1">
            {{ t('auth.password') }}
          </label>
          <div class="relative">
            <Lock class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
                v-model="form.password"
                type="password"
                required
                class="w-full pl-12 pr-4 py-4 bg-gray-50 dark:bg-gray-900 border border-gray-100 dark:border-gray-800 rounded-2xl focus:ring-2 focus:ring-restall-green outline-none transition-all text-restall-dark dark:text-restall-light font-bold"
            >
          </div>
        </div>

        <div class="space-y-2">
          <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 dark:text-gray-500 ml-1">
            {{ t('auth.confirm_password') }}
          </label>
          <div class="relative">
            <LockKeyhole class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
                v-model="form.password_confirmation"
                type="password"
                required
                class="w-full pl-12 pr-4 py-4 bg-gray-50 dark:bg-gray-900 border border-gray-100 dark:border-gray-800 rounded-2xl focus:ring-2 focus:ring-restall-green outline-none transition-all text-restall-dark dark:text-restall-light font-bold"
            >
          </div>
        </div>

        <BaseButton type="submit" variant="primary" size="lg" class="w-full py-4 mt-4" :disabled="loading">
          <Loader2 v-if="loading" class="w-5 h-5 animate-spin mr-2" />
          {{ t('auth.submit_register') }}
        </BaseButton>
      </form>

      <div class="mt-8 text-center">
        <p class="text-sm font-medium text-gray-500 dark:text-gray-400">
          {{ t('auth.have_account') }}
          <router-link to="/login" class="font-black text-restall-green hover:text-restall-dark dark:hover:text-restall-light ml-1 transition-colors">
            {{ t('auth.to_login') }}
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { Mail, Lock, User, LockKeyhole, Loader2, AlertCircle } from 'lucide-vue-next';
import { useAuthStore } from '@/stores/auth.store';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const router = useRouter();
const authStore = useAuthStore();
const loading = ref(false);
const errorMessage = ref('');

const form = reactive({
  name: '',
  email: '',
  password: '',
  password_confirmation: ''
});

const handleRegister = async () => {
  loading.value = true;
  errorMessage.value = '';

  try {
    await authStore.register(form);
    router.push({ path: '/login', query: { registered: 'true' } });
  } catch (e: any) {
    errorMessage.value = e.response?.data?.message || 'Registration failed. Please check your inputs.';
  } finally {
    loading.value = false;
  }
};
</script>