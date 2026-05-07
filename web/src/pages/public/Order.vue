<template>
  <div class="min-h-screen bg-gray-50 dark:bg-restall-dark pb-32">
    <div v-if="loading" class="flex h-screen items-center justify-center">
      <Loader2 class="w-12 h-12 animate-spin text-restall-gold" />
    </div>

    <div v-else-if="currentStep === 'mode_selection'" class="max-w-xl mx-auto pt-20 px-6 animate-fade-in-up">
      <header class="mb-12 text-left">
        <h1 class="text-5xl font-black uppercase tracking-tighter dark:text-white leading-[0.85]">
          Start your <br/><span class="text-restall-gold">Experience</span>
        </h1>
      </header>

      <div class="space-y-4">
        <button
            @click="handleModeSelection('dine_in')"
            :disabled="!orderContext?.has_active_reservation"
            :class="['w-full p-8 rounded-[2.5rem] border-2 transition-all flex items-center justify-between',
            orderContext?.has_active_reservation ? 'bg-white dark:bg-gray-800 border-transparent hover:border-restall-green' : 'opacity-40 grayscale cursor-not-allowed bg-gray-100 dark:bg-gray-900 border-transparent']"
        >
          <div class="text-left">
            <span class="block text-2xl font-black uppercase dark:text-white">{{ t('order.type.dine_in') }}</span>
            <span v-if="orderContext?.has_active_reservation" class="text-restall-green text-[10px] font-black uppercase tracking-widest">
              {{ orderContext.context?.restaurant_name }} • Table {{ orderContext.context?.table_number }}
            </span>
            <span v-else class="text-gray-400 text-[10px] font-black uppercase tracking-widest">
              No active reservation found
            </span>
          </div>
          <Utensils class="w-10 h-10 text-restall-green" />
        </button>

        <button
            @click="handleModeSelection('take_away')"
            class="w-full p-8 bg-white dark:bg-gray-800 rounded-[2.5rem] border-2 border-transparent hover:border-restall-gold transition-all flex items-center justify-between"
        >
          <div class="text-left">
            <span class="block text-2xl font-black uppercase dark:text-white">{{ t('order.type.take_away') }}</span>
            <span class="text-gray-400 text-[10px] font-black uppercase tracking-widest">Pick up at restaurant</span>
          </div>
          <ShoppingBag class="w-10 h-10 text-restall-gold" />
        </button>
      </div>
    </div>

    <div v-else-if="currentStep === 'restaurant_selection'" class="max-w-xl mx-auto pt-20 px-6 animate-fade-in-up">
      <header class="mb-12 text-left">
        <h1 class="text-4xl font-black uppercase dark:text-white tracking-tighter">Select <span class="text-restall-gold">Location</span></h1>
      </header>
      <div class="space-y-4 max-h-[60vh] overflow-y-auto pr-2 custom-scrollbar">
        <div
            v-for="res in restaurants" :key="res.id"
            @click="selectRestaurant(res)"
            class="p-6 bg-white dark:bg-gray-800 rounded-3xl cursor-pointer hover:border-restall-gold border-2 border-transparent transition-all text-left"
        >
          <h4 class="font-black uppercase dark:text-white">{{ res.name }}</h4>
          <p class="text-xs text-gray-400 font-bold uppercase">{{ res.address }}</p>
        </div>
      </div>
    </div>

    <template v-else>
      <header class="bg-white dark:bg-gray-800/50 sticky top-0 z-30 border-b border-gray-100 dark:border-gray-700 backdrop-blur-md">
        <div class="max-w-7xl mx-auto px-4 h-20 flex items-center justify-between">
          <div class="flex items-center gap-4 text-left">
            <div class="w-12 h-12 bg-restall-gold/10 rounded-2xl flex items-center justify-center text-restall-gold">
              <Utensils v-if="orderType === 'dine_in'" class="w-6 h-6" />
              <ShoppingBag v-else class="w-6 h-6" />
            </div>
            <div>
              <h2 class="text-lg font-black dark:text-white uppercase leading-none">{{ selectedRestaurant?.name }}</h2>
              <span class="text-[10px] font-black text-gray-400 uppercase tracking-widest">
                {{ orderType === 'dine_in' ? `Table ${orderContext?.context?.table_number}` : 'Take Away' }}
              </span>
            </div>
          </div>
          <BaseButton variant="ghost" @click="currentStep = 'mode_selection'" class="text-[10px] font-black uppercase">{{ t('common.back') }}</BaseButton>
        </div>
      </header>

      <main class="max-w-7xl mx-auto px-4 py-8">
        <div class="flex gap-4 overflow-x-auto pb-6 no-scrollbar">
          <button
              v-for="cat in categories" :key="cat.id"
              @click="activeCategory = cat.id"
              :class="['px-6 py-3 rounded-full text-[10px] font-black uppercase tracking-widest transition-all whitespace-nowrap',
              activeCategory === cat.id ? 'bg-restall-dark text-white dark:bg-restall-gold' : 'bg-white dark:bg-gray-800 text-gray-400']"
          >
            {{ cat.name }}
          </button>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div v-for="item in menuItems" :key="item.id" class="bg-white dark:bg-gray-800/40 p-4 rounded-[2.5rem] flex gap-4 border border-transparent hover:border-gray-100 dark:hover:border-gray-700 transition-all group">
            <div class="w-24 h-24 rounded-3xl overflow-hidden bg-gray-100 flex-shrink-0">
              <img :src="item.image_url" class="w-full h-full object-cover group-hover:scale-110 transition-transform" />
            </div>
            <div class="flex flex-col justify-between py-1 flex-1 text-left">
              <div>
                <h4 class="font-black uppercase text-xs dark:text-white">{{ item.name }}</h4>
                <p class="text-[10px] text-gray-400 font-medium line-clamp-2">{{ item.description }}</p>
              </div>
              <div class="flex items-center justify-between">
                <span class="font-black text-restall-green">${{ item.price }}</span>
                <button class="w-8 h-8 bg-restall-dark dark:bg-restall-gold text-white rounded-xl flex items-center justify-center active:scale-90 transition-transform">
                  <Plus class="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </main>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { Utensils, ShoppingBag, Loader2, Plus } from 'lucide-vue-next';
import { OrdersService, type OrderContext } from '@/api/orders.service';
import { MenuService } from '@/api/menu.service';
import { RestaurantsService } from '@/api/restaurants.service';
import BaseButton from '@/components/UI/BaseButton.vue';

interface Restaurant {
  id: number;
  name: string;
  address?: string;
}

interface Category {
  id: number;
  name: string;
}

interface MenuItem {
  id: number;
  name: string;
  description: string;
  price: string | number;
  image_url: string;
}

const { t } = useI18n();
const route = useRoute();
const router = useRouter();

const loading = ref<boolean>(true);
const currentStep = ref<'mode_selection' | 'restaurant_selection' | 'menu'>('mode_selection');
const orderContext = ref<OrderContext | null>(null);
const orderType = ref<'dine_in' | 'take_away' | null>(null);

const restaurants = ref<Restaurant[]>([]);
const selectedRestaurant = ref<Restaurant | null>(null);
const categories = ref<Category[]>([]);
const menuItems = ref<MenuItem[]>([]);
const activeCategory = ref<number | null>(null);

const fetchInitialData = async (): Promise<void> => {
  try {
    const response = await OrdersService.getContext();
    orderContext.value = response.data;

    if (route.params.restaurantId) {
      const resData = await RestaurantsService.show(route.params.restaurantId as string);
      selectedRestaurant.value = resData.data || resData;
    }
  } catch (e) {
    router.push({ name: 'Login' });
  } finally {
    loading.value = false;
  }
};

const handleModeSelection = async (mode: 'dine_in' | 'take_away'): Promise<void> => {
  orderType.value = mode;

  if (mode === 'dine_in' && orderContext.value?.has_active_reservation) {
    selectedRestaurant.value = {
      id: orderContext.value.context!.restaurant_id,
      name: orderContext.value.context!.restaurant_name
    };
    await loadMenu();
    currentStep.value = 'menu';
  } else {
    if (selectedRestaurant.value) {
      await loadMenu();
      currentStep.value = 'menu';
    } else {
      loading.value = true;
      const res = await RestaurantsService.index();
      restaurants.value = res.data || res;
      currentStep.value = 'restaurant_selection';
      loading.value = false;
    }
  }
};

const selectRestaurant = async (res: Restaurant): Promise<void> => {
  selectedRestaurant.value = res;
  await loadMenu();
  currentStep.value = 'menu';
};

const loadMenu = async (): Promise<void> => {
  if (!selectedRestaurant.value) return;
  try {
    const [catRes, itemRes] = await Promise.all([
      MenuService.getCategories(selectedRestaurant.value.id),
      MenuService.getItems(selectedRestaurant.value.id, activeCategory.value || undefined)
    ]);
    categories.value = catRes.data || catRes;
    menuItems.value = itemRes.data || itemRes;

    if (categories.value.length > 0 && !activeCategory.value) {
      activeCategory.value = categories.value[0].id;
    }
  } catch (e) {
    console.error(e);
  }
};

watch(activeCategory, () => {
  loadMenu();
});

onMounted(fetchInitialData);
</script>