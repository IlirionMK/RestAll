import { ref } from 'vue';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

interface Toast {
    id: number;
    message: string;
    type: ToastType;
}

let nextId = 0;
const toasts = ref<Toast[]>([]);

export function useToast() {
    const dismiss = (id: number) => {
        const index = toasts.value.findIndex(t => t.id === id);
        if (index !== -1) toasts.value.splice(index, 1);
    };

    const add = (message: string, type: ToastType, duration = 4000) => {
        const id = nextId++;
        toasts.value.push({ id, message, type });
        setTimeout(() => dismiss(id), duration);
    };

    return {
        toasts,
        dismiss,
        success: (msg: string) => add(msg, 'success'),
        error: (msg: string) => add(msg, 'error'),
        warning: (msg: string) => add(msg, 'warning'),
        info: (msg: string) => add(msg, 'info'),
    };
}
