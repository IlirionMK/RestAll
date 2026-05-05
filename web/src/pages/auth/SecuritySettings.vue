<template>
  <div class="max-w-3xl mx-auto p-6 space-y-8 font-sans">
    <div class="bg-white dark:bg-gray-800 p-8 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-700">
      <h2 class="text-2xl font-black text-gray-900 dark:text-white mb-2">
        {{ t('security.2fa_title') }}
      </h2>
      <p class="text-gray-500 text-sm mb-8">
        {{ t('security.2fa_desc') }}
      </p>

      <div v-if="!is2FAEnabled && !isSetupMode">
        <BaseButton @click="startSetup" variant="primary" :disabled="loading">
          {{ t('security.enable_2fa') }}
        </BaseButton>
      </div>

      <div v-else-if="isSetupMode" class="space-y-6">
        <div class="p-6 bg-blue-50 dark:bg-blue-900/20 border border-blue-100 dark:border-blue-800 rounded-xl">
          <h3 class="font-bold text-blue-900 dark:text-blue-100 mb-4">{{ t('security.step1_title') }}</h3>
          <p class="text-sm text-blue-800 dark:text-blue-200 mb-4">
            {{ t('security.step1_desc') }}
          </p>
          <div class="bg-white p-4 inline-block rounded-xl shadow-sm" v-html="qrCodeSvg"></div>
        </div>

        <div class="space-y-4">
          <h3 class="font-bold text-gray-900 dark:text-white">{{ t('security.step2_title') }}</h3>
          <form @submit.prevent="confirmSetup" class="flex gap-4 items-end">
            <div class="flex-1 space-y-2">
              <label class="text-[10px] font-black uppercase tracking-widest text-gray-400 ml-1">{{ t('security.app_code') }}</label>
              <input v-model="confirmationCode" type="text" required maxlength="6" class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-xl outline-none font-bold text-center tracking-[0.5em] text-xl">
            </div>
            <BaseButton type="submit" variant="primary" class="py-3 px-8" :disabled="loading || confirmationCode.length !== 6">
              {{ t('security.confirm') }}
            </BaseButton>
          </form>
          <p v-if="errorMessage" class="text-rose-500 text-sm font-bold">{{ errorMessage }}</p>
        </div>

        <div class="pt-4 flex justify-end">
          <button @click="cancelSetup" class="text-sm text-gray-500 hover:text-gray-700 font-semibold">{{ t('security.cancel') }}</button>
        </div>
      </div>

      <div v-else-if="is2FAEnabled" class="space-y-8">
        <div class="flex items-center gap-3 text-emerald-600 bg-emerald-50 dark:bg-emerald-900/20 p-4 rounded-xl border border-emerald-100 dark:border-emerald-800">
          <ShieldCheck class="w-6 h-6" />
          <span class="font-bold">{{ t('security.2fa_enabled_badge') }}</span>
        </div>

        <div class="space-y-4">
          <h3 class="font-bold text-gray-900 dark:text-white">{{ t('security.recovery_codes_title') }}</h3>
          <p class="text-sm text-gray-500">
            {{ t('security.recovery_codes_desc') }}
          </p>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-3 bg-gray-50 dark:bg-gray-900 p-6 rounded-xl border border-gray-200 dark:border-gray-700">
            <code v-for="code in recoveryCodes" :key="code" class="text-sm font-mono bg-white dark:bg-gray-800 px-3 py-2 rounded shadow-sm text-center">
              {{ code }}
            </code>
          </div>
        </div>

        <div class="pt-6 border-t border-gray-100 dark:border-gray-800">
          <BaseButton @click="disable2FA" variant="danger" :disabled="loading">
            {{ t('security.disable_2fa') }}
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { ShieldCheck } from 'lucide-vue-next';
import { AuthService } from '@/api/auth.service';
import BaseButton from '@/components/UI/BaseButton.vue';
import { useAuthStore } from '@/stores/auth.store';

const { t } = useI18n();
const authStore = useAuthStore();

const is2FAEnabled = ref(false);
const isSetupMode = ref(false);
const loading = ref(false);
const qrCodeSvg = ref('');
const recoveryCodes = ref<string[]>([]);
const confirmationCode = ref('');
const errorMessage = ref('');

onMounted(async () => {
  is2FAEnabled.value = !!authStore.user?.two_factor_confirmed_at;
  if (is2FAEnabled.value) {
    await loadRecoveryCodes();
  }
});

const startSetup = async () => {
  loading.value = true;
  try {
    await AuthService.enable2FA();
    const qrResponse = await AuthService.get2FAQR();
    qrCodeSvg.value = qrResponse.svg;
    isSetupMode.value = true;
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const confirmSetup = async () => {
  loading.value = true;
  errorMessage.value = '';
  try {
    await AuthService.confirm2FA({ code: confirmationCode.value });
    isSetupMode.value = false;
    is2FAEnabled.value = true;
    await loadRecoveryCodes();
    await authStore.fetchUser();
  } catch (e: any) {
    errorMessage.value = e.response?.data?.message || t('security.invalid_code');
  } finally {
    loading.value = false;
    confirmationCode.value = '';
  }
};

const cancelSetup = async () => {
  await AuthService.disable2FA();
  isSetupMode.value = false;
  qrCodeSvg.value = '';
  confirmationCode.value = '';
}

const loadRecoveryCodes = async () => {
  try {
    const response = await AuthService.get2FARecoveryCodes();
    recoveryCodes.value = response;
  } catch (e) {
    console.error(e);
  }
};

const disable2FA = async () => {
  if (!confirm(t('security.disable_confirm'))) return;

  loading.value = true;
  try {
    await AuthService.disable2FA();
    is2FAEnabled.value = false;
    recoveryCodes.value = [];
    await authStore.fetchUser();
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};
</script>