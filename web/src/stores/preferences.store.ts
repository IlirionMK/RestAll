import { defineStore } from 'pinia';
import { ref } from 'vue';
import { i18n } from '@/i18n';

export const usePreferencesStore = defineStore('preferences', () => {
    const locale = ref(localStorage.getItem('locale') ?? 'en');
    const theme = ref(localStorage.getItem('theme') ?? 'dark');

    function applyTheme(t: string) {
        document.documentElement.classList.toggle('dark', t === 'dark');
    }

    function setLocale(lang: string) {
        locale.value = lang;
        localStorage.setItem('locale', lang);
        (i18n.global.locale as any).value = lang;
    }

    function setTheme(t: string) {
        theme.value = t;
        localStorage.setItem('theme', t);
        applyTheme(t);
    }

    applyTheme(theme.value);

    return { locale, theme, setLocale, setTheme };
});
