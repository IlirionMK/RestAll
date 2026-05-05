<template>
  <div class="max-w-md mx-auto w-full py-12 px-6 font-sans">
    <div class="bg-white dark:bg-gray-800 p-8 md:p-10 rounded-squircle shadow-2xl shadow-restall-green/5 border border-gray-100 dark:border-gray-700">
      <div class="text-center mb-10">
        <h1 class="text-3xl font-black text-restall-dark dark:text-restall-light uppercase tracking-tighter">
          {{ authStore.requires2FA ? t('auth.2fa_title') : t('auth.login_title') }}
        </h1>
        <p v-if="!authStore.requires2FA" class="mt-2 text-sm text-gray-500 dark:text-gray-400 font-medium">
          {{ t('auth.login_subtitle') }}
        </p>
      </div>

      <div v-if="successMessage" class="mb-6 p-4 bg-restall-green/10 border border-restall-green/20 rounded-2xl flex items-start text-restall-green text-sm font-bold transition-all">
        <CheckCircle2 class="w-5 h-5 mr-3 flex-shrink-0" />
        <p>{{ successMessage }}</p>
      </div>

      <div v-if="errorMessage" class="mb-6 p-4 bg-rose-500/10 border border-rose-500/20 rounded-2xl flex items-start text-rose-500 text-sm font-bold transition-all">
        <AlertCircle class="w-5 h-5 mr-3 flex-shrink-0" />
        <p>{{ errorMessage }}</p>
      </div>

      <form v-if="!authStore.requires2FA" @submit.prevent="handleLogin" class="space-y-6">
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

        <BaseButton type="submit" variant="primary" size="lg" class="w-full py-4 mt-4" :disabled="loading">
          <Loader2 v-if="loading" class="w-5 h-5 animate-spin mr-2" />
          {{ t('auth.to_login') }}
        </BaseButton>
      </form>

      <form v-else @submit.prevent="handle2FA" class="space-y-6">
        <div class="space-y-2 text-center">
          <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 dark:text-gray-500">
            {{ t('auth.2fa_code') }}
          </label>
          <div class="relative mt-2">
            <ShieldCheck class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
                v-model="twoFactorCode"
                type="text"
                required
                maxlength="6"
                pattern="\d*"
                class="w-full pl-12 pr-4 py-4 bg-gray-50 dark:bg-gray-900 border border-gray-100 dark:border-gray-800 rounded-2xl focus:ring-2 focus:ring-restall-green outline-none transition-all text-restall-dark dark:text-restall-light font-bold text-center tracking-[0.5em] text-2xl"
                placeholder="000000"
            >
          </div>
        </div>

        <BaseButton type="submit" variant="primary" size="lg" class="w-full py-4 mt-4" :disabled="loading">
          <Loader2 v-if="loading" class="w-5 h-5 animate-spin mr-2" />
          {{ t('auth.verify') }}
        </BaseButton>

        <button
            type="button"
            @click="authStore.requires2FA = false"
            class="w-full text-sm font-bold text-gray-400 hover:text-restall-green transition-colors"
        >
          {{ t('auth.back_to_login') }}
        </button>
      </form>

      <div v-if="!authStore.requires2FA" class="mt-8 text-center">
        <p class="text-sm font-medium text-gray-500 dark:text-gray-400">
          {{ t('auth.no_account') }}
          <router-link to="/register" class="font-black text-restall-green hover:text-restall-dark dark:hover:text-restall-light ml-1 transition-colors">
            {{ t('auth.to_register') }}
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { Mail, Lock, Loader2, AlertCircle, CheckCircle2, ShieldCheck } from 'lucide-vue-next';
import { useAuthStore } from '@/stores/auth.store';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const loading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');
const twoFactorCode = ref('');

const form = reactive({
  email: '',
  password: ''
});

onMounted(() => {
  if (route.query.registered === 'true') {
    successMessage.value = t('auth.registration_success');
    router.replace({ query: {} });
  }
});

const routeRedirect = () => {
  if (authStore.userRole === 'admin') {
    router.push('/admin');
  } else if (authStore.userRole === 'chef') {
    router.push('/kitchen');
  } else {
    router.push('/');
  }
};

const handleLogin = async () => {
  loading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    await authStore.login(form);
    if (!authStore.requires2FA) {
      routeRedirect();
    }
  } catch (e: any) {
    errorMessage.value = e.response?.data?.message || t('auth.error_credentials');
  } finally {
    loading.value = false;
  }
};

const handle2FA = async () => {
  loading.value = true;
  errorMessage.value = '';

  try {
    await authStore.verify2FA(twoFactorCode.value);
    routeRedirect();
  } catch (e: any) {
    errorMessage.value = e.response?.data?.message || t('auth.error_2fa');
  } finally {
    loading.value = false;
    twoFactorCode.value = '';
  }
};
</script>