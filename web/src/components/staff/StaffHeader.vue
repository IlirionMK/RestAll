<template>
  <header :class="['h-16 border-b px-4 sm:px-6 flex items-center justify-between sticky top-0 z-20', th.header]">
    <div class="flex items-center gap-3 sm:gap-4">
      <div :class="['w-9 h-9 rounded-2xl flex items-center justify-center flex-shrink-0 shadow-sm', th.logoBg]">
        <span :class="['text-sm font-black', th.logoText]">R</span>
      </div>
      <div class="leading-none hidden sm:block">
        <p :class="['text-sm font-black uppercase tracking-widest', th.brandName]">RestAll</p>
        <p :class="['text-[10px] font-black uppercase tracking-widest', th.brandLabel]">{{ brandLabel }}</p>
      </div>
      <nav v-if="navItems.length" class="flex items-center gap-1 sm:ml-2">
        <RouterLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="px-3 py-1.5 rounded-xl text-[11px] font-black uppercase tracking-widest transition-colors"
            :class="route.path === item.to ? th.navActive : th.navInactive"
        >
          {{ item.label }}
        </RouterLink>
      </nav>
    </div>

    <div class="flex items-center gap-1">
      <button @click="notifOpen = true" :class="['relative p-2.5 rounded-xl transition-colors', th.iconBtn]">
        <Bell :class="['w-5 h-5', th.iconColor]" />
        <span
            v-if="notifStore.unreadCount > 0"
            class="absolute top-1.5 right-1.5 min-w-[16px] h-4 px-0.5 bg-[#C29B40] rounded-full text-[9px] font-black text-white flex items-center justify-center leading-none"
        >
          {{ notifStore.unreadCount > 9 ? '9+' : notifStore.unreadCount }}
        </span>
      </button>
      <button @click="burgerOpen = true" :class="['p-2.5 rounded-xl transition-colors', th.iconBtn]">
        <MenuIcon :class="['w-5 h-5', th.iconColor]" />
      </button>
    </div>
  </header>

  <!-- Notification panel -->
  <Teleport to="body">
    <Transition enter-active-class="transition duration-200" enter-from-class="opacity-0" enter-to-class="opacity-100"
                leave-active-class="transition duration-150" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="notifOpen" class="fixed inset-0 z-50 flex justify-end">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="notifOpen = false" />
        <div :class="['relative w-80 h-full border-l flex flex-col shadow-2xl', th.panel, th.panelBorder]">
          <div :class="['flex items-center justify-between px-5 py-4 border-b', th.panelBorder]">
            <div class="flex items-center gap-2">
              <h2 :class="['text-sm font-black uppercase tracking-widest', th.panelTitle]">{{ t('staff.notifications') }}</h2>
              <span v-if="notifStore.unreadCount > 0" class="px-2 py-0.5 bg-[#C29B40] rounded-full text-[10px] font-black text-white leading-none">
                {{ notifStore.unreadCount }}
              </span>
            </div>
            <div class="flex items-center gap-3">
              <button v-if="notifStore.items.length > 0" @click="notifStore.markAllRead()"
                      :class="['text-[10px] font-black uppercase tracking-widest transition-colors', th.panelSubText]">
                {{ t('staff.mark_all_read') }}
              </button>
              <button @click="notifOpen = false" :class="['p-1 rounded-lg transition-colors', th.iconBtn]">
                <X :class="['w-4 h-4', th.iconColor]" />
              </button>
            </div>
          </div>

          <div class="flex-1 overflow-y-auto">
            <div v-if="notifStore.items.length === 0" class="flex flex-col items-center justify-center h-full gap-3 px-6 text-center">
              <Bell :class="['w-10 h-10', th.emptyIcon]" />
              <p :class="['text-sm font-black uppercase tracking-widest', th.emptyIcon]">{{ t('staff.no_notifications') }}</p>
            </div>
            <div v-else :class="['divide-y', th.divide]">
              <div v-for="notif in notifStore.items" :key="notif.id"
                   :class="['flex items-start gap-3 px-5 py-4 transition-colors', notif.read ? 'opacity-40' : th.notifUnread]">
                <div :class="['w-8 h-8 rounded-xl flex items-center justify-center flex-shrink-0', notifBg(notif.type)]">
                  <component :is="notifIcon(notif.type)" class="w-4 h-4" :class="notifColor(notif.type)" />
                </div>
                <div class="flex-1 min-w-0">
                  <p :class="['text-sm font-bold leading-snug', th.panelTitle]">{{ notif.message }}</p>
                  <p :class="['text-[10px] font-medium mt-0.5', th.panelSubText]">{{ timeAgo(notif.timestamp) }}</p>
                </div>
                <div v-if="!notif.read" class="w-2 h-2 rounded-full bg-[#C29B40] mt-1.5 flex-shrink-0" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- Burger panel -->
  <Teleport to="body">
    <Transition enter-active-class="transition duration-200" enter-from-class="opacity-0" enter-to-class="opacity-100"
                leave-active-class="transition duration-150" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="burgerOpen" class="fixed inset-0 z-50 flex justify-end">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="burgerOpen = false" />
        <div :class="['relative w-72 h-full border-l flex flex-col shadow-2xl overflow-y-auto', th.panel, th.panelBorder]">

          <div :class="['px-5 pt-6 pb-5 border-b', th.panelBorder]">
            <div class="flex items-center gap-3 mb-4">
              <div :class="['w-10 h-10 rounded-2xl flex items-center justify-center flex-shrink-0', th.avatarBg]">
                <span :class="['text-sm font-black', th.avatarText]">{{ initials }}</span>
              </div>
              <div class="min-w-0">
                <p :class="['text-sm font-black truncate', th.panelTitle]">{{ authStore.user?.name }}</p>
                <p :class="['text-[10px] font-medium truncate', th.panelSubText]">{{ authStore.user?.email }}</p>
              </div>
            </div>

            <button @click="editingProfile = !editingProfile"
                    :class="['w-full flex items-center justify-between py-2.5 px-3 rounded-xl transition-colors', th.iconBtn]">
              <span :class="['text-[11px] font-black uppercase tracking-widest', th.panelSubText]">{{ t('staff.edit_profile') }}</span>
              <ChevronDown :class="['w-3.5 h-3.5 transition-transform duration-200', th.panelSubText, editingProfile ? 'rotate-180' : '']" />
            </button>

            <div v-if="editingProfile" class="mt-2 space-y-2">
              <input v-model="profileName" type="text" :placeholder="t('staff.your_name')"
                     :class="['w-full px-3 py-2.5 border rounded-xl text-sm font-medium focus:outline-none transition-colors', th.input]" />
              <button @click="saveProfile" :disabled="savingProfile || !profileName.trim()"
                      class="w-full py-2.5 bg-[#14532D] text-white rounded-xl text-[11px] font-black uppercase tracking-widest hover:opacity-90 transition-opacity disabled:opacity-50">
                {{ savingProfile ? t('staff.saving') : t('staff.save') }}
              </button>
            </div>
          </div>

          <div :class="['px-5 py-4 border-b', th.panelBorder]">
            <p :class="['text-[10px] font-black uppercase tracking-widest mb-3', th.panelSubText]">{{ t('staff.language') }}</p>
            <div class="flex gap-2">
              <button v-for="lang in ['en', 'pl']" :key="lang" @click="prefStore.setLocale(lang)"
                      :class="['flex-1 py-2 rounded-xl text-[11px] font-black uppercase tracking-widest transition-colors',
                               prefStore.locale === lang ? 'bg-[#14532D] text-white' : th.toggleBtn]">
                {{ lang.toUpperCase() }}
              </button>
            </div>
          </div>

          <div :class="['px-5 py-4 border-b', th.panelBorder]">
            <p :class="['text-[10px] font-black uppercase tracking-widest mb-3', th.panelSubText]">{{ t('staff.theme') }}</p>
            <div class="flex gap-2">
              <button v-for="th2 in themes" :key="th2.key" @click="prefStore.setTheme(th2.key)"
                      :class="['flex-1 py-2 rounded-xl text-[11px] font-black uppercase tracking-widest transition-colors',
                               prefStore.theme === th2.key ? 'bg-[#14532D] text-white' : th.toggleBtn]">
                {{ th2.label }}
              </button>
            </div>
          </div>

          <div class="flex-1" />

          <div class="px-5 py-5">
            <button @click="handleLogout"
                    class="w-full py-3 flex items-center justify-center gap-2 bg-rose-500/10 text-rose-500 rounded-2xl text-[11px] font-black uppercase tracking-widest hover:bg-rose-500/20 transition-colors">
              <LogOut class="w-4 h-4" />
              {{ t('staff.sign_out') }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { Bell, Menu as MenuIcon, X, ChevronDown, LogOut, Banknote, Utensils, ShoppingBag } from 'lucide-vue-next';
import { useAuthStore } from '@/stores/auth.store';
import { useNotificationsStore } from '@/stores/notifications.store';
import { usePreferencesStore } from '@/stores/preferences.store';
import { API } from '@/api';
import type { NotifType } from '@/stores/notifications.store';

const props = withDefaults(defineProps<{
  brandLabel: string;
  variant?: 'light' | 'dark';
  navItems?: { label: string; to: string }[];
}>(), {
  variant: 'dark',
  navItems: () => [],
});

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const notifStore = useNotificationsStore();
const prefStore = usePreferencesStore();

const notifOpen = ref(false);
const burgerOpen = ref(false);
const editingProfile = ref(false);
const profileName = ref(authStore.user?.name ?? '');
const savingProfile = ref(false);

const th = computed(() => {
  if (props.variant === 'light') {
    return {
      header:      'bg-white border-gray-100',
      logoBg:      'bg-[#14532D]',
      logoText:    'text-white',
      brandName:   'text-[#1F2937]',
      brandLabel:  'text-[#14532D]',
      navActive:   'bg-gray-100 text-[#1F2937]',
      navInactive: 'text-[#1F2937]/50 hover:text-[#1F2937] hover:bg-gray-100/70',
      iconBtn:     'hover:bg-gray-100',
      iconColor:   'text-[#1F2937]/50',
      panel:       'bg-[#FAF9F6]',
      panelBorder: 'border-gray-100',
      panelTitle:  'text-[#1F2937]',
      panelSubText:'text-[#1F2937]/50',
      emptyIcon:   'text-[#1F2937]/20',
      divide:      'divide-gray-100',
      notifUnread: 'bg-white',
      avatarBg:    'bg-[#14532D]/10',
      avatarText:  'text-[#14532D]',
      input:       'bg-white border-gray-200 text-[#1F2937] placeholder-gray-400 focus:border-[#14532D]',
      toggleBtn:   'bg-gray-100 text-[#1F2937]/50 hover:bg-gray-200',
    };
  }
  return {
    header:      'bg-gray-900 border-gray-800',
    logoBg:      'bg-[#C29B40]/20',
    logoText:    'text-[#C29B40]',
    brandName:   'text-white',
    brandLabel:  'text-[#C29B40]',
    navActive:   'bg-gray-800 text-white',
    navInactive: 'text-gray-500 hover:text-white hover:bg-gray-800/60',
    iconBtn:     'hover:bg-gray-800',
    iconColor:   'text-gray-400',
    panel:       'bg-gray-900',
    panelBorder: 'border-gray-800',
    panelTitle:  'text-white',
    panelSubText:'text-gray-500',
    emptyIcon:   'text-gray-700',
    divide:      'divide-gray-800/60',
    notifUnread: 'bg-gray-800/20',
    avatarBg:    'bg-[#C29B40]/20',
    avatarText:  'text-[#C29B40]',
    input:       'bg-gray-800 border-gray-700 text-white placeholder-gray-600 focus:border-[#C29B40]',
    toggleBtn:   'bg-gray-800 text-gray-400 hover:bg-gray-700',
  };
});

const initials = computed(() => {
  const name = authStore.user?.name ?? '';
  return name.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
});

const themes = computed(() => [
  { key: 'dark', label: t('staff.theme_dark') },
  { key: 'light', label: t('staff.theme_light') },
]);

const notifIcon = (type: NotifType) => type === 'billing_requested' ? Banknote : type === 'kitchen_ready' ? Utensils : ShoppingBag;
const notifBg   = (type: NotifType) => type === 'billing_requested' ? 'bg-[#C29B40]/20' : type === 'kitchen_ready' ? 'bg-[#14532D]/20' : 'bg-blue-500/20';
const notifColor = (type: NotifType) => type === 'billing_requested' ? 'text-[#C29B40]' : type === 'kitchen_ready' ? 'text-[#14532D]' : 'text-blue-400';

const timeAgo = (date: Date) => {
  const diff = Math.floor((Date.now() - date.getTime()) / 60000);
  return diff < 1 ? t('notif.just_now') : t('notif.min_ago', { n: diff });
};

const saveProfile = async () => {
  if (!profileName.value.trim()) return;
  savingProfile.value = true;
  try {
    const { data } = await API.user.updateMe({ name: profileName.value.trim() });
    if (authStore.user) authStore.user.name = data.name;
    editingProfile.value = false;
  } finally {
    savingProfile.value = false;
  }
};

const handleLogout = async () => {
  burgerOpen.value = false;
  await authStore.logout();
  router.push('/');
};
</script>
