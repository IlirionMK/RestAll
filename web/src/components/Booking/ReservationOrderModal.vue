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
      <div v-if="modelValue" class="fixed inset-0 z-50 flex items-end sm:items-center justify-center p-0 sm:p-4" @click.self="$emit('update:modelValue', false)">
        <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="$emit('update:modelValue', false)" />

        <div class="relative w-full sm:max-w-2xl max-h-[95dvh] sm:max-h-[90vh] flex flex-col bg-white dark:bg-gray-900 rounded-t-[2.5rem] sm:rounded-[2.5rem] shadow-2xl overflow-hidden">

          <div class="flex-shrink-0 px-8 pt-8 pb-6 border-b border-gray-100 dark:border-gray-800">
            <div class="flex items-start justify-between gap-4">
              <div>
                <div :class="['inline-flex px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest mb-3', statusBadgeClass]">
                  {{ t(`bookings.status_${booking.status}`) }}
                </div>
                <h2 class="text-2xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
                  {{ booking.restaurant?.name }}
                </h2>
                <p class="text-sm font-bold text-gray-400 mt-1">
                  {{ t('bookings.table_label_short') }} №{{ booking.table?.number }} &middot; {{ formattedDate }}
                </p>
              </div>
              <button @click="$emit('update:modelValue', false)" class="p-2 rounded-2xl hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors flex-shrink-0">
                <X class="w-5 h-5 text-gray-400" />
              </button>
            </div>
          </div>

          <div class="flex-1 overflow-y-auto">

            <div v-if="loading" class="flex items-center justify-center py-20">
              <Loader2 class="w-8 h-8 animate-spin text-restall-gold" />
            </div>

            <template v-else>

              <div v-if="!order" class="px-8 py-8 space-y-6">
                <p class="text-sm font-bold text-gray-400 uppercase tracking-widest">{{ t('order.no_order_yet') }}</p>

                <label class="flex items-center justify-between p-5 bg-gray-50 dark:bg-gray-800 rounded-2xl cursor-pointer group">
                  <div>
                    <p class="font-black text-restall-dark dark:text-restall-light text-sm uppercase tracking-wide">{{ t('order.takeaway_label') }}</p>
                    <p class="text-xs text-gray-400 font-medium mt-0.5">{{ t('order.takeaway_hint') }}</p>
                  </div>
                  <div
                      @click="isTakeaway = !isTakeaway"
                      :class="['relative w-12 h-6 rounded-full transition-colors duration-200 cursor-pointer', isTakeaway ? 'bg-restall-green' : 'bg-gray-200 dark:bg-gray-700']"
                  >
                    <span :class="['absolute top-1 w-4 h-4 bg-white rounded-full shadow transition-transform duration-200', isTakeaway ? 'translate-x-7' : 'translate-x-1']" />
                  </div>
                </label>

                <BaseButton variant="primary" size="lg" class="w-full py-4" :disabled="creating" @click="handleStartOrder">
                  <Loader2 v-if="creating" class="w-4 h-4 animate-spin mr-2" />
                  {{ t('order.start_order') }}
                </BaseButton>
              </div>

              <template v-else>

                <div class="px-8 pt-6 pb-4">
                  <div class="flex items-center justify-between mb-4">
                    <h3 class="text-[11px] font-black uppercase tracking-widest text-gray-400">{{ t('order.items_in_order') }}</h3>
                    <div class="flex items-center gap-2">
                      <span v-if="order.is_takeaway" class="px-3 py-1 bg-restall-gold/10 text-restall-gold rounded-full text-[10px] font-black uppercase tracking-widest">
                        {{ t('order.type.take_away') }}
                      </span>
                      <span :class="['px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest', orderStatusClass]">
                        {{ t(`order.status_${order.status}`) }}
                      </span>
                    </div>
                  </div>

                  <div v-if="order.items?.length" class="space-y-2">
                    <div
                        v-for="item in order.items"
                        :key="item.id"
                        class="flex items-center justify-between py-3 border-b border-gray-50 dark:border-gray-800"
                    >
                      <div class="flex-1 min-w-0">
                        <p class="font-black text-sm text-restall-dark dark:text-restall-light truncate">{{ item.name }}</p>
                        <p class="text-xs text-gray-400 font-medium">x{{ item.quantity }}</p>
                      </div>
                      <p class="font-black text-sm text-restall-dark dark:text-restall-light ml-4">
                        {{ formatPrice(item.price * item.quantity) }}
                      </p>
                    </div>
                  </div>
                  <p v-else class="text-sm text-gray-400 font-medium py-2">{{ t('order.cart_empty') }}</p>

                  <div class="flex items-center justify-between pt-4 mt-2 border-t border-gray-100 dark:border-gray-800">
                    <span class="text-[11px] font-black uppercase tracking-widest text-gray-400">{{ t('order.total') }}</span>
                    <span class="text-xl font-black text-restall-dark dark:text-restall-light">{{ formatPrice(order.total_amount) }}</span>
                  </div>
                </div>

                <div v-if="order.status === 'pending'" class="px-8 pb-6">
                  <button
                      @click="menuOpen = !menuOpen"
                      class="w-full flex items-center justify-between py-4 px-5 bg-gray-50 dark:bg-gray-800 rounded-2xl font-black text-sm uppercase tracking-widest text-restall-dark dark:text-restall-light hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                  >
                    {{ t('order.add_items') }}
                    <ChevronDown :class="['w-4 h-4 transition-transform', menuOpen ? 'rotate-180' : '']" />
                  </button>

                  <div v-if="menuOpen" class="mt-4 space-y-6">
                    <div v-if="menuLoading" class="flex justify-center py-8">
                      <Loader2 class="w-6 h-6 animate-spin text-restall-gold" />
                    </div>
                    <template v-else>
                      <div v-for="category in menu" :key="category.id" class="space-y-2">
                        <h4 class="text-[10px] font-black uppercase tracking-widest text-gray-400">{{ category.name }}</h4>
                        <div
                            v-for="item in category.items"
                            :key="item.id"
                            class="flex items-center justify-between p-4 bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl"
                        >
                          <div class="flex-1 min-w-0 mr-4">
                            <p class="font-black text-sm text-restall-dark dark:text-restall-light">{{ item.name }}</p>
                            <p class="text-xs text-restall-gold font-black mt-0.5">{{ formatPrice(item.price) }}</p>
                          </div>
                          <button
                              @click="addItem(item)"
                              :disabled="addingItem === item.id"
                              class="flex-shrink-0 w-8 h-8 flex items-center justify-center bg-restall-green text-white rounded-xl hover:bg-restall-dark transition-colors disabled:opacity-50"
                          >
                            <Loader2 v-if="addingItem === item.id" class="w-3 h-3 animate-spin" />
                            <Plus v-else class="w-4 h-4" />
                          </button>
                        </div>
                      </div>
                    </template>
                  </div>
                </div>

                <div v-if="order.status === 'pending' && order.items?.length" class="px-8 pb-8">
                  <BaseButton
                      variant="primary"
                      size="lg"
                      class="w-full py-4"
                      :disabled="requestingBill"
                      @click="handleRequestBill"
                  >
                    <Loader2 v-if="requestingBill" class="w-4 h-4 animate-spin mr-2" />
                    {{ t('order.request_bill') }}
                  </BaseButton>
                </div>

                <div v-if="order.status === 'billing_requested'" class="px-8 pb-8">
                  <div class="w-full py-4 text-center bg-restall-gold/10 text-restall-gold font-black text-sm uppercase tracking-widest rounded-2xl">
                    {{ t('order.bill_requested') }}
                  </div>
                </div>

              </template>
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
import { X, Loader2, ChevronDown, Plus } from 'lucide-vue-next';
import { ReservationsService } from '@/api/reservations.service';
import { OrdersService } from '@/api/orders.service';
import { MenuService } from '@/api/menu.service';
import BaseButton from '@/components/UI/BaseButton.vue';

const props = defineProps<{
  modelValue: boolean;
  booking: any;
}>();

const emit = defineEmits<{
  'update:modelValue': [value: boolean];
}>();

const { t } = useI18n();

const loading = ref(false);
const creating = ref(false);
const requestingBill = ref(false);
const menuLoading = ref(false);
const menuOpen = ref(false);
const addingItem = ref<number | null>(null);
const isTakeaway = ref(false);
const order = ref<any>(null);
const menu = ref<any[]>([]);

const formattedDate = computed(() =>
    new Date(props.booking.reservation_time).toLocaleString('en-GB', {
      day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit'
    })
);

const statusBadgeClass = computed(() => ({
  confirmed: 'bg-restall-green/10 text-restall-green',
  pending: 'bg-restall-gold/10 text-restall-gold',
  cancelled: 'bg-rose-100 text-rose-500',
  completed: 'bg-gray-100 text-gray-500',
}[props.booking.status] ?? 'bg-gray-100 text-gray-400'));

const orderStatusClass = computed(() => ({
  pending: 'bg-restall-gold/10 text-restall-gold',
  billing_requested: 'bg-blue-100 text-blue-500',
  paid: 'bg-green-100 text-green-600',
}[order.value?.status] ?? 'bg-gray-100 text-gray-400'));

const formatPrice = (value: number | string) =>
    Number(value).toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });

const fetchOrder = async () => {
  loading.value = true;
  try {
    order.value = await ReservationsService.getOrder(props.booking.id);
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

const handleStartOrder = async () => {
  creating.value = true;
  try {
    order.value = await ReservationsService.createOrder(props.booking.id, isTakeaway.value);
  } finally {
    creating.value = false;
  }
};

const addItem = async (item: any) => {
  addingItem.value = item.id;
  try {
    await OrdersService.addItems(order.value.id, [{ menu_item_id: item.id, quantity: 1, price: item.price }]);
    order.value = await ReservationsService.getOrder(props.booking.id);
  } finally {
    addingItem.value = null;
  }
};

const handleRequestBill = async () => {
  requestingBill.value = true;
  try {
    const { data } = await OrdersService.requestBill(order.value.id);
    order.value = data;
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
    isTakeaway.value = false;
    menu.value = [];
  }
});

watch(menuOpen, (open) => {
  if (open) fetchMenu();
});
</script>
