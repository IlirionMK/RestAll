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

        <div class="relative my-8">
          <div class="absolute inset-0 flex items-center">
            <span class="w-full border-t border-gray-100 dark:border-gray-700"></span>
          </div>
          <div class="relative flex justify-center text-[10px] uppercase tracking-widest font-black">
            <span class="bg-white dark:bg-gray-800 px-4 text-gray-400 uppercase">
              {{ t('auth.or_continue_with') }}
            </span>
          </div>
        </div>

        <button
            @click="loginWithGoogle"
            type="button"
            class="w-full flex items-center justify-center gap-3 bg-white dark:bg-gray-900 border border-gray-100 dark:border-gray-700 hover:border-restall-green py-4 px-6 rounded-2xl transition-all group shadow-sm"
        >
          <svg class="w-5 h-5" viewBox="0 0 24 24">
            <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
            <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
            <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z" fill="#FBBC05"/>
            <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
          </svg>
          <span class="font-black uppercase text-[10px] tracking-widest text-restall-dark dark:text-restall-light group-hover:text-restall-green transition-colors">
            Google
          </span>
        </button>
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
    errorMessage.value = e.response?.data?.message || t('auth.error_registration');
  } finally {
    loading.value = false;
  }
};

const loginWithGoogle = () => {
  authStore.loginWithGoogle();
};
</script>