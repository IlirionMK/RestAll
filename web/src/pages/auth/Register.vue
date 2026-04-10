<template>
  <div class="max-w-md mx-auto w-full py-12 px-4">
    <div class="space-y-8">
      <div class="text-center">
        <h1 class="text-3xl font-black text-gray-900 dark:text-white">
          {{ t('auth.register_title') }}
        </h1>
        <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
          {{ t('auth.register_subtitle') }}
        </p>
      </div>

      <form @submit.prevent="handleRegister" class="space-y-6">
        <div class="space-y-1">
          <label class="text-xs font-black uppercase tracking-widest text-gray-400 ml-1">
            {{ t('auth.name') }}
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <User class="h-5 w-5 text-gray-400" />
            </div>
            <input
                v-model="form.name"
                type="text"
                required
                class="block w-full pl-11 pr-4 py-3 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all dark:text-white"
            >
          </div>
        </div>

        <div class="space-y-1">
          <label class="text-xs font-black uppercase tracking-widest text-gray-400 ml-1">
            {{ t('auth.email') }}
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <Mail class="h-5 w-5 text-gray-400" />
            </div>
            <input
                v-model="form.email"
                type="email"
                required
                class="block w-full pl-11 pr-4 py-3 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all dark:text-white"
            >
          </div>
        </div>

        <div class="space-y-1">
          <label class="text-xs font-black uppercase tracking-widest text-gray-400 ml-1">
            {{ t('auth.password') }}
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <Lock class="h-5 w-5 text-gray-400" />
            </div>
            <input
                v-model="form.password"
                type="password"
                required
                class="block w-full pl-11 pr-4 py-3 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all dark:text-white"
            >
          </div>
        </div>

        <div class="space-y-1">
          <label class="text-xs font-black uppercase tracking-widest text-gray-400 ml-1">
            {{ t('auth.confirm_password') }}
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <LockKeyhole class="h-5 w-5 text-gray-400" />
            </div>
            <input
                v-model="form.password_confirmation"
                type="password"
                required
                class="block w-full pl-11 pr-4 py-3 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all dark:text-white"
            >
          </div>
        </div>

        <BaseButton
            type="submit"
            variant="primary"
            size="lg"
            class="w-full py-4 shadow-xl shadow-emerald-600/20"
            :disabled="loading"
        >
          <Loader2 v-if="loading" class="w-5 h-5 animate-spin mr-2" />
          {{ t('auth.submit_register') }}
        </BaseButton>
      </form>

      <div class="text-center">
        <p class="text-sm text-gray-500 dark:text-gray-400">
          {{ t('auth.have_account') }}
          <router-link to="/login" class="font-black text-emerald-600 hover:text-emerald-500 ml-1">
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
import { Mail, Lock, User, LockKeyhole, Loader2 } from 'lucide-vue-next';
import api, { getCsrfCookie } from '@/api/axios';
import BaseButton from '@/components/UI/BaseButton.vue';

const { t } = useI18n();
const router = useRouter();
const loading = ref(false);

const form = reactive({
  name: '',
  email: '',
  password: '',
  password_confirmation: ''
});

const handleRegister = async () => {
  loading.value = true;
  try {
    await getCsrfCookie();
    await api.post('/api/auth/register', form);
    router.push('/login');
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};
</script>