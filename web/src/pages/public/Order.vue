<template>
  <div class="min-h-screen bg-gray-50 dark:bg-restall-dark">

    <div v-if="loading" class="flex h-screen items-center justify-center">
      <Loader2 class="w-12 h-12 animate-spin text-restall-gold" />
    </div>

    <div v-else-if="step === 'restaurant_selection'" class="max-w-xl mx-auto pt-16 px-6 pb-20 animate-fade-in-up">
      <h1 class="text-5xl font-black uppercase tracking-tighter dark:text-white leading-[0.9] mb-3">
        Our <span class="text-restall-gold">Locations</span>
      </h1>
      <p class="text-gray-400 font-medium mb-10">Select a restaurant to browse the menu.</p>

      <div class="space-y-4">
        <button
            v-for="res in restaurants"
            :key="res.id"
            @click="selectRestaurant(res)"
            class="w-full text-left p-6 bg-white dark:bg-gray-800 rounded-3xl border-2 border-transparent hover:border-restall-gold transition-all group"
        >
          <div class="flex items-center justify-between">
            <div>
              <h4 class="font-black text-lg uppercase dark:text-white group-hover:text-restall-gold transition-colors">{{ res.name }}</h4>
              <p v-if="res.address" class="text-xs text-gray-400 font-bold uppercase mt-1">{{ res.address }}</p>
            </div>
            <ChevronRight class="w-5 h-5 text-gray-300 group-hover:text-restall-gold transition-colors" />
          </div>
        </button>
        <div v-if="restaurants.length === 0" class="text-center py-12 text-gray-400 font-medium">
          No restaurants found
        </div>
      </div>
    </div>

    <template v-else>
      <header class="bg-white/80 dark:bg-gray-900/80 sticky top-0 z-30 border-b border-gray-100 dark:border-gray-800 backdrop-blur-md">
        <div class="max-w-7xl mx-auto px-4 h-16 flex items-center justify-between gap-4">
          <div class="flex items-center gap-3 min-w-0">
            <button @click="step = 'restaurant_selection'" class="p-2 rounded-xl hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors flex-shrink-0">
              <ChevronLeft class="w-5 h-5 text-gray-500" />
            </button>
            <div class="min-w-0">
              <h2 class="font-black uppercase text-sm dark:text-white truncate leading-tight">{{ selectedRestaurant?.name }}</h2>
              <span v-if="canOrder" class="text-[10px] font-black text-restall-green uppercase tracking-widest">
                Table {{ orderContext?.context?.table_number }} · Ordering active
              </span>
              <span v-else class="text-[10px] font-black text-gray-400 uppercase tracking-widest">
                Browsing menu
              </span>
            </div>
          </div>

          <div v-if="canOrder && cartCount > 0" class="flex-shrink-0">
            <button
                @click="cartOpen = !cartOpen"
                class="flex items-center gap-2 px-4 py-2 bg-restall-dark dark:bg-restall-gold text-white rounded-2xl font-black text-xs uppercase tracking-widest"
            >
              <ShoppingBag class="w-4 h-4" />
              {{ cartCount }}
            </button>
          </div>
        </div>

        <div class="max-w-7xl mx-auto px-4 pb-3">
          <div class="flex gap-2 overflow-x-auto no-scrollbar">
            <button
                v-for="cat in categories"
                :key="cat.id"
                @click="activeCategory = cat.id"
                :class="['px-5 py-2 rounded-full text-[10px] font-black uppercase tracking-widest transition-all whitespace-nowrap flex-shrink-0',
                activeCategory === cat.id
                  ? 'bg-restall-dark text-white dark:bg-restall-gold'
                  : 'bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700']"
            >
              {{ cat.name }}
            </button>
          </div>
        </div>
      </header>

      <main class="max-w-7xl mx-auto px-4 py-8 pb-40">
        <div v-if="menuLoading" class="flex justify-center py-20">
          <Loader2 class="w-8 h-8 animate-spin text-restall-gold" />
        </div>

        <div v-else-if="menuItems.length === 0" class="text-center py-20 text-gray-400 font-medium">
          No items in this category
        </div>

        <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
          <div
              v-for="item in menuItems"
              :key="item.id"
              class="bg-white dark:bg-gray-800 rounded-3xl overflow-hidden border border-gray-100 dark:border-gray-700 flex flex-col group hover:shadow-lg transition-shadow"
          >
            <div class="aspect-[4/3] bg-gray-100 dark:bg-gray-700 overflow-hidden">
              <img
                  v-if="item.photo_url"
                  :src="item.photo_url"
                  :alt="item.name"
                  class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
              />
              <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 dark:bg-gray-700">
                <Utensils class="w-10 h-10 text-gray-300 dark:text-gray-600" />
              </div>
            </div>

            <div class="p-5 flex flex-col flex-1">
              <h4 class="font-black text-sm uppercase dark:text-white leading-tight">{{ item.name }}</h4>
              <p v-if="item.description" class="text-xs text-gray-400 font-medium mt-1.5 line-clamp-2 flex-1">{{ item.description }}</p>
              <div v-else class="flex-1" />

              <div class="flex items-center justify-between mt-4">
                <span class="font-black text-restall-green text-base">{{ formatPrice(item.price) }}</span>

                <div v-if="canOrder" class="flex items-center gap-2">
                  <template v-if="cartQuantity(item.id) > 0">
                    <button
                        @click="removeFromCart(item.id)"
                        class="w-7 h-7 rounded-lg bg-gray-100 dark:bg-gray-700 flex items-center justify-center hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
                    >
                      <Minus class="w-3.5 h-3.5 text-gray-600 dark:text-gray-300" />
                    </button>
                    <span class="text-sm font-black dark:text-white w-4 text-center">{{ cartQuantity(item.id) }}</span>
                  </template>
                  <button
                      @click="addToCart(item)"
                      class="w-7 h-7 rounded-lg bg-restall-dark dark:bg-restall-gold text-white flex items-center justify-center hover:opacity-80 transition-opacity active:scale-90"
                  >
                    <Plus class="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>

      <div
          v-if="canOrder && cartCount > 0"
          class="fixed bottom-0 left-0 right-0 z-40 p-4"
      >
        <div class="max-w-7xl mx-auto">
          <div class="bg-restall-dark dark:bg-restall-gold rounded-3xl shadow-2xl overflow-hidden">
            <Transition
                enter-active-class="transition duration-200 ease-out"
                enter-from-class="opacity-0 -translate-y-2"
                enter-to-class="opacity-100 translate-y-0"
                leave-active-class="transition duration-150"
                leave-from-class="opacity-100 translate-y-0"
                leave-to-class="opacity-0 -translate-y-2"
            >
              <div v-if="cartOpen" class="px-6 pt-5 pb-2 border-b border-white/10">
                <div class="space-y-2 max-h-48 overflow-y-auto">
                  <div v-for="ci in cart" :key="ci.item.id" class="flex items-center justify-between">
                    <span class="text-white text-sm font-bold truncate flex-1 mr-3">{{ ci.item.name }}</span>
                    <div class="flex items-center gap-2 flex-shrink-0">
                      <button @click="removeFromCart(ci.item.id)" class="w-6 h-6 rounded-md bg-white/20 flex items-center justify-center hover:bg-white/30 transition-colors">
                        <Minus class="w-3 h-3 text-white" />
                      </button>
                      <span class="text-white text-sm font-black w-4 text-center">{{ ci.quantity }}</span>
                      <button @click="addToCart(ci.item)" class="w-6 h-6 rounded-md bg-white/20 flex items-center justify-center hover:bg-white/30 transition-colors">
                        <Plus class="w-3 h-3 text-white" />
                      </button>
                    </div>
                    <span class="text-white/70 text-xs font-black ml-3">{{ formatPrice(Number(ci.item.price) * ci.quantity) }}</span>
                  </div>
                </div>
              </div>
            </Transition>

            <div class="flex items-center justify-between px-6 py-4 gap-4">
              <button @click="cartOpen = !cartOpen" class="flex items-center gap-3 flex-1 min-w-0">
                <div class="w-8 h-8 rounded-xl bg-white/20 flex items-center justify-center flex-shrink-0">
                  <ShoppingBag class="w-4 h-4 text-white" />
                </div>
                <div class="text-left min-w-0">
                  <p class="text-white text-[10px] font-black uppercase tracking-widest leading-none">{{ cartCount }} item{{ cartCount !== 1 ? 's' : '' }}</p>
                  <p class="text-white font-black text-sm leading-tight">{{ formatPrice(cartTotal) }}</p>
                </div>
                <ChevronUp :class="['w-4 h-4 text-white/60 transition-transform ml-auto', cartOpen ? 'rotate-180' : '']" />
              </button>

              <button
                  @click="placeOrder"
                  :disabled="placing"
                  class="flex items-center gap-2 px-6 py-3 bg-white text-restall-dark font-black text-xs uppercase tracking-widest rounded-2xl hover:bg-gray-100 transition-colors disabled:opacity-50 flex-shrink-0"
              >
                <Loader2 v-if="placing" class="w-4 h-4 animate-spin" />
                Place Order
              </button>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Loader2, Plus, Minus, ShoppingBag, ChevronRight, ChevronLeft, ChevronUp, Utensils } from 'lucide-vue-next';
import { OrdersService, type OrderContext } from '@/api/orders.service';
import { ReservationsService } from '@/api/reservations.service';
import { MenuService } from '@/api/menu.service';
import { RestaurantsService } from '@/api/restaurants.service';
import { useToast } from '@/composables/useToast';

interface Restaurant { id: number; name: string; address?: string; }
interface MenuItem { id: number; name: string; description?: string; price: number | string; photo_url?: string; }
interface CategoryWithItems { id: number; name: string; items: MenuItem[]; }
interface CartItem { item: MenuItem; quantity: number; }

const route = useRoute();
const router = useRouter();
const { success, error: toastError } = useToast();

const loading = ref(true);
const menuLoading = ref(false);
const placing = ref(false);
const step = ref<'restaurant_selection' | 'menu'>('restaurant_selection');
const orderContext = ref<OrderContext | null>(null);
const restaurants = ref<Restaurant[]>([]);
const selectedRestaurant = ref<Restaurant | null>(null);
const categoriesWithItems = ref<CategoryWithItems[]>([]);
const activeCategory = ref<number | null>(null);
const cart = ref<CartItem[]>([]);
const cartOpen = ref(false);

const categories = computed(() => categoriesWithItems.value.map(({ id, name }) => ({ id, name })));
const menuItems = computed(() => {
  const cat = categoriesWithItems.value.find(c => c.id === activeCategory.value);
  return cat?.items ?? [];
});

const canOrder = computed(() =>
    orderContext.value?.has_active_reservation &&
    orderContext.value.context?.restaurant_id === selectedRestaurant.value?.id
);

const cartCount = computed(() => cart.value.reduce((s, ci) => s + ci.quantity, 0));
const cartTotal = computed(() => cart.value.reduce((s, ci) => s + Number(ci.item.price) * ci.quantity, 0));
const cartQuantity = (itemId: number) => cart.value.find(ci => ci.item.id === itemId)?.quantity ?? 0;

const formatPrice = (value: number | string) =>
    Number(value).toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });

const loadMenu = async () => {
  if (!selectedRestaurant.value) return;
  menuLoading.value = true;
  try {
    const { data } = await MenuService.getCategories(selectedRestaurant.value.id);
    categoriesWithItems.value = data;
    if (data.length > 0) activeCategory.value = data[0].id;
  } finally {
    menuLoading.value = false;
  }
};

const selectRestaurant = async (res: Restaurant) => {
  selectedRestaurant.value = res;
  step.value = 'menu';
  cart.value = [];
  cartOpen.value = false;
  await loadMenu();
};

const addToCart = (item: MenuItem) => {
  const existing = cart.value.find(ci => ci.item.id === item.id);
  if (existing) existing.quantity++;
  else cart.value.push({ item, quantity: 1 });
};

const removeFromCart = (itemId: number) => {
  const idx = cart.value.findIndex(ci => ci.item.id === itemId);
  if (idx === -1) return;
  if (cart.value[idx].quantity > 1) cart.value[idx].quantity--;
  else cart.value.splice(idx, 1);
};

const placeOrder = async () => {
  const context = orderContext.value?.context;
  if (!context) return;

  placing.value = true;
  try {
    let order: any;
    try {
      order = await ReservationsService.getOrder(context.reservation_id);
    } catch {
      order = await ReservationsService.createOrder(context.reservation_id);
    }

    await OrdersService.addItems(order.id, cart.value.map(ci => ({
      menu_item_id: ci.item.id,
      quantity: ci.quantity,
      price: ci.item.price,
    })));

    cart.value = [];
    cartOpen.value = false;
    success('Order sent to kitchen!');
  } catch {
    toastError('Failed to place order');
  } finally {
    placing.value = false;
  }
};

onMounted(async () => {
  try {
    const [contextRes, restaurantsRes] = await Promise.all([
      OrdersService.getContext(),
      RestaurantsService.index(),
    ]);
    orderContext.value = contextRes.data;
    restaurants.value = restaurantsRes.data || restaurantsRes;

    const routeId = route.params.restaurantId as string | undefined;
    if (routeId) {
      const found = restaurants.value.find(r => String(r.id) === String(routeId));
      if (found) {
        await selectRestaurant(found);
        loading.value = false;
        return;
      }
    }

    if (orderContext.value?.has_active_reservation && orderContext.value.context) {
      const ctx = orderContext.value.context;
      const active = restaurants.value.find(r => r.id === ctx.restaurant_id);
      if (active) {
        await selectRestaurant(active);
        loading.value = false;
        return;
      }
    }
  } catch {
    router.push({ name: 'Login' });
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.no-scrollbar::-webkit-scrollbar { display: none; }
.no-scrollbar { -ms-overflow-style: none; scrollbar-width: none; }
@keyframes fade-in-up { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
.animate-fade-in-up { animation: fade-in-up 0.4s ease-out forwards; }
</style>
