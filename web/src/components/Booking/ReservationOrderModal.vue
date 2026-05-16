<template>
  <Teleport to="body">
    <Transition
        enter-active-class="transition duration-200 ease-out"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition duration-150 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
    >
      <div v-if="modelValue" class="fixed inset-0 z-50 flex items-end sm:items-center justify-center p-0 sm:p-4" @click.self="close">
        <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="close" />

        <div class="relative w-full sm:max-w-lg max-h-[95dvh] sm:max-h-[88vh] flex flex-col bg-white dark:bg-gray-900 rounded-t-[2.5rem] sm:rounded-[2.5rem] shadow-2xl overflow-hidden">

          <div class="flex-shrink-0 px-6 pt-6 pb-5 border-b border-gray-100 dark:border-gray-800">
            <div class="flex items-start justify-between gap-4">
              <div class="min-w-0">
                <span :class="['inline-flex text-[10px] font-black uppercase tracking-widest px-3 py-1 rounded-full mb-3', reservationBadgeClass]">
                  {{ t(`bookings.status_${booking.status}`) }}
                </span>
                <h2 class="text-xl font-black text-restall-dark dark:text-restall-light truncate">
                  {{ booking.restaurant?.name }}
                </h2>
                <p class="text-sm text-gray-400 font-medium mt-0.5">
                  {{ t('bookings.table_label_short') }} №{{ booking.table?.number }} &middot; {{ formattedDate }}
                </p>
              </div>
              <button @click="close" class="p-2 rounded-2xl hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors flex-shrink-0 mt-1">
                <X class="w-5 h-5 text-gray-400" />
              </button>
            </div>
          </div>

          <div class="flex-1 overflow-y-auto">

            <div v-if="loading" class="flex items-center justify-center py-20">
              <Loader2 class="w-8 h-8 animate-spin text-restall-gold" />
            </div>

            <div v-else-if="error" class="m-6 p-4 bg-rose-50 dark:bg-rose-500/10 border border-rose-200 dark:border-rose-500/20 rounded-2xl text-rose-500 text-sm font-bold">
              {{ error }}
            </div>

            <template v-else-if="!order">
              <div class="flex flex-col items-center justify-center py-16 px-8 text-center">
                <div class="w-20 h-20 rounded-3xl bg-restall-gold/10 flex items-center justify-center mb-5">
                  <Clock class="w-10 h-10 text-restall-gold" />
                </div>
                <h3 class="text-xl font-black text-restall-dark dark:text-restall-light mb-2">
                  {{ t('order.waiting_title') }}
                </h3>
                <p class="text-sm text-gray-400 font-medium leading-relaxed max-w-xs">
                  {{ t('order.waiting_desc') }}
                </p>
              </div>
            </template>

            <template v-else>

              <div class="px-6 pt-6 pb-4">
                <div class="flex items-center justify-between mb-5">
                  <h3 class="text-[11px] font-black uppercase tracking-widest text-gray-400">
                    {{ t('order.items_in_order') }}
                  </h3>
                  <div class="flex items-center gap-2">
                    <span v-if="order.is_takeaway" class="px-2.5 py-1 bg-restall-gold/10 text-restall-gold rounded-full text-[10px] font-black uppercase tracking-widest">
                      {{ t('order.type.take_away') }}
                    </span>
                    <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest', orderStatusClass]">
                      {{ t(`order.status_${order.status}`) }}
                    </span>
                  </div>
                </div>

                <div v-if="!order.items?.length" class="py-10 text-center text-gray-400 text-sm font-medium border-2 border-dashed border-gray-100 dark:border-gray-800 rounded-2xl">
                  {{ t('order.cart_empty') }}
                </div>

                <div v-else class="space-y-3">
                  <div
                      v-for="item in order.items"
                      :key="item.id"
                      class="flex items-center gap-3"
                  >
                    <div class="w-14 h-14 rounded-2xl overflow-hidden bg-gray-100 dark:bg-gray-800 flex-shrink-0">
                      <img
                          v-if="item.menuItem?.photo_url"
                          :src="item.menuItem.photo_url"
                          :alt="item.name"
                          class="w-full h-full object-cover"
                      />
                      <div v-else class="w-full h-full flex items-center justify-center">
                        <Utensils class="w-5 h-5 text-gray-300 dark:text-gray-600" />
                      </div>
                    </div>

                    <div class="flex-1 min-w-0">
                      <p class="font-bold text-sm text-restall-dark dark:text-restall-light truncate">{{ item.name }}</p>
                      <p class="text-xs text-gray-400 font-medium">x{{ item.quantity }}</p>
                    </div>

                    <p class="font-black text-sm text-restall-dark dark:text-restall-light flex-shrink-0">
                      {{ formatPrice(Number(item.price) * item.quantity) }}
                    </p>
                  </div>

                  <div class="flex items-center justify-between pt-4 mt-1 border-t border-gray-100 dark:border-gray-800">
                    <span class="text-[11px] font-black uppercase tracking-widest text-gray-400">{{ t('order.total') }}</span>
                    <span class="text-2xl font-black text-restall-dark dark:text-restall-light">{{ formatPrice(order.total_amount) }}</span>
                  </div>
                </div>
              </div>

              <div v-if="order.status === 'pending'" class="px-6 pb-4">
                <button
                    @click="menuOpen = !menuOpen"
                    class="w-full flex items-center justify-between py-4 px-5 bg-gray-50 dark:bg-gray-800 rounded-2xl font-black text-sm uppercase tracking-widest text-restall-dark dark:text-restall-light hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                >
                  {{ t('order.add_items') }}
                  <ChevronDown :class="['w-4 h-4 text-gray-400 transition-transform', menuOpen ? 'rotate-180' : '']" />
                </button>

                <div v-if="menuOpen" class="mt-3 space-y-5">
                  <div v-if="menuLoading" class="flex justify-center py-8">
                    <Loader2 class="w-6 h-6 animate-spin text-restall-gold" />
                  </div>
                  <template v-else>
                    <div v-for="cat in menu" :key="cat.id">
                      <p class="text-[10px] font-black uppercase tracking-widest text-gray-400 mb-2 px-1">{{ cat.name }}</p>
                      <div class="space-y-2">
                        <div
                            v-for="menuItem in cat.items"
                            :key="menuItem.id"
                            class="flex items-center gap-3 p-3 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl"
                        >
                          <div class="w-10 h-10 rounded-xl overflow-hidden bg-gray-100 dark:bg-gray-700 flex-shrink-0">
                            <img v-if="menuItem.photo_url" :src="menuItem.photo_url" class="w-full h-full object-cover" />
                            <div v-else class="w-full h-full flex items-center justify-center">
                              <Utensils class="w-4 h-4 text-gray-300" />
                            </div>
                          </div>
                          <div class="flex-1 min-w-0">
                            <p class="font-bold text-sm text-restall-dark dark:text-restall-light truncate">{{ menuItem.name }}</p>
                            <p class="text-xs text-restall-gold font-black">{{ formatPrice(menuItem.price) }}</p>
                          </div>
                          <button
                              @click="addItem(menuItem)"
                              :disabled="addingItem === menuItem.id"
                              class="w-8 h-8 bg-restall-green text-white rounded-xl flex items-center justify-center hover:opacity-80 transition-opacity disabled:opacity-50 flex-shrink-0"
                          >
                            <Loader2 v-if="addingItem === menuItem.id" class="w-3 h-3 animate-spin" />
                            <Plus v-else class="w-4 h-4" />
                          </button>
                        </div>
                      </div>
                    </div>
                  </template>
                </div>
              </div>

              <div v-if="order.status === 'pending' && order.items?.length" class="px-6 pb-6">
                <BaseButton variant="primary" size="lg" class="w-full rounded-2xl py-4" :disabled="requestingBill" @click="handleRequestBill">
                  <Loader2 v-if="requestingBill" class="w-4 h-4 animate-spin mr-2" />
                  {{ t('order.request_bill') }}
                </BaseButton>
              </div>

              <div v-if="order.status === 'billing_requested'" class="px-6 pb-6">
                <div class="p-5 bg-restall-gold/10 border border-restall-gold/20 rounded-2xl text-center space-y-1">
                  <p class="text-restall-gold font-black text-sm uppercase tracking-widest">{{ t('order.bill_requested') }}</p>
                  <p class="text-gray-400 text-xs font-medium">{{ t('order.bill_on_way') }}</p>
                </div>
              </div>

            </template>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { X, Loader2, ChevronDown, Plus, Clock, Utensils } from 'lucide-vue-next';
import { ReservationsService } from '@/api/reservations.service';
import { OrdersService } from '@/api/orders.service';
import { MenuService } from '@/api/menu.service';
import BaseButton from '@/components/UI/BaseButton.vue';

const props = defineProps<{ modelValue: boolean; booking: any }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>();

const { t } = useI18n();

const loading = ref(false);
const requestingBill = ref(false);
const menuLoading = ref(false);
const menuOpen = ref(false);
const addingItem = ref<number | null>(null);
const order = ref<any>(null);
const menu = ref<any[]>([]);
const error = ref<string | null>(null);

const close = () => emit('update:modelValue', false);

const formattedDate = computed(() =>
    new Date(props.booking.reservation_time).toLocaleString('en-GB', {
      day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit'
    })
);

const reservationBadgeClass = computed(() => ({
  confirmed: 'bg-restall-green/10 text-restall-green',
  pending: 'bg-restall-gold/10 text-restall-gold',
  cancelled: 'bg-rose-100 text-rose-500',
  completed: 'bg-gray-100 text-gray-500',
}[props.booking.status] ?? 'bg-gray-100 text-gray-400'));

const orderStatusClass = computed(() => ({
  pending: 'bg-restall-gold/10 text-restall-gold',
  billing_requested: 'bg-blue-100 text-blue-500 dark:bg-blue-500/10',
  paid: 'bg-green-100 text-green-600',
}[order.value?.status] ?? 'bg-gray-100 text-gray-400'));

const formatPrice = (value: number | string) =>
    Number(value).toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });

const fetchOrder = async () => {
  loading.value = true;
  error.value = null;
  try {
    order.value = await ReservationsService.getOrder(props.booking.id);
  } catch {
    order.value = null;
  } finally {
    loading.value = false;
  }
};

const fetchMenu = async () => {
  if (menu.value.length) return;
  menuLoading.value = true;
  try {
    const { data } = await MenuService.getCategories(props.booking.restaurant_id);
    menu.value = data;
  } finally {
    menuLoading.value = false;
  }
};

const addItem = async (item: any) => {
  addingItem.value = item.id;
  error.value = null;
  try {
    await OrdersService.addItems(order.value.id, [{ menu_item_id: item.id, quantity: 1, price: item.price }]);
    order.value = await ReservationsService.getOrder(props.booking.id);
  } catch {
    error.value = t('errors.booking_failed');
  } finally {
    addingItem.value = null;
  }
};

const handleRequestBill = async () => {
  requestingBill.value = true;
  error.value = null;
  try {
    const { data } = await OrdersService.requestBill(order.value.id);
    order.value = data;
  } catch {
    error.value = t('errors.booking_failed');
  } finally {
    requestingBill.value = false;
  }
};

watch(() => props.modelValue, (open) => {
  if (open) {
    fetchOrder();
  } else {
    order.value = null;
    menuOpen.value = false;
    menu.value = [];
    error.value = null;
  }
});

watch(menuOpen, (open) => {
  if (open) fetchMenu();
});
</script>
