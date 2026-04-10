<template>
  <div>
    <h1 class="text-2xl font-bold text-gray-900 dark:text-white">{{ t('admin.menu') }}</h1>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
</script>