<template>
  <div class="space-y-10 font-sans">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
        Menu Management
      </h1>
      <BaseButton variant="primary" @click="openAddModal">
        <Plus class="w-5 h-5 mr-2" />
        Add Item
      </BaseButton>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <Loader2 class="w-10 h-10 animate-spin text-restall-gold" />
    </div>

    <template v-else>
      <div class="bg-white dark:bg-gray-800 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
        <div class="p-6 border-b border-gray-100 dark:border-gray-700 flex flex-col sm:flex-row gap-4">
          <input
              v-model="search"
              type="text"
              placeholder="Search menu items..."
              class="flex-1 px-4 py-3 bg-gray-50 dark:bg-gray-900 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light"
          />
          <select
              v-model="activeCategoryId"
              class="px-4 py-3 bg-gray-50 dark:bg-gray-900 rounded-2xl outline-none font-bold text-gray-500 cursor-pointer"
          >
            <option :value="null">All Categories</option>
            <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
          </select>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left">
            <thead>
              <tr class="bg-gray-50 dark:bg-gray-900/50">
                <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Item</th>
                <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Category</th>
                <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Price</th>
                <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400">Status</th>
                <th class="px-6 py-4 text-[10px] font-black uppercase tracking-widest text-gray-400 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
              <tr v-if="filteredItems.length === 0">
                <td colspan="5" class="px-6 py-16 text-center text-gray-400 font-medium">No items found</td>
              </tr>
              <tr
                  v-for="item in filteredItems"
                  :key="item.id"
                  class="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
              >
                <td class="px-6 py-4">
                  <p class="font-bold text-restall-dark dark:text-restall-light">{{ item.name }}</p>
                  <p v-if="item.description" class="text-xs text-gray-500 mt-1 line-clamp-1">{{ item.description }}</p>
                </td>
                <td class="px-6 py-4 font-bold text-gray-600 dark:text-gray-300">{{ item.category_name }}</td>
                <td class="px-6 py-4 font-black text-restall-gold">{{ formatPrice(item.price) }}</td>
                <td class="px-6 py-4">
                  <button
                      @click="toggleAvailability(item)"
                      :disabled="togglingId === item.id"
                      :class="['px-3 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest transition-colors disabled:opacity-50',
                      item.is_available
                        ? 'bg-restall-green/10 text-restall-green hover:bg-restall-green/20'
                        : 'bg-gray-100 dark:bg-gray-700 text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-600']"
                  >
                    <Loader2 v-if="togglingId === item.id" class="w-3 h-3 animate-spin inline mr-1" />
                    {{ item.is_available ? 'Available' : 'Unavailable' }}
                  </button>
                </td>
                <td class="px-6 py-4">
                  <div class="flex justify-end gap-2">
                    <BaseButton variant="ghost" size="sm" class="px-3 rounded-xl" @click="openEditModal(item)">
                      <Edit class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton
                        variant="ghost"
                        size="sm"
                        class="px-3 rounded-xl text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10"
                        @click="removeItem(item.id)"
                    >
                      <Trash2 class="w-4 h-4" />
                    </BaseButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <Teleport to="body">
      <Transition
          enter-active-class="transition duration-200 ease-out"
          enter-from-class="opacity-0"
          enter-to-class="opacity-100"
          leave-active-class="transition duration-150"
          leave-from-class="opacity-100"
          leave-to-class="opacity-0"
      >
        <div v-if="modalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4" @click.self="closeModal">
          <div class="absolute inset-0 bg-black/50 backdrop-blur-sm" @click="closeModal" />
          <div class="relative w-full max-w-md bg-white dark:bg-gray-900 rounded-[2.5rem] shadow-2xl p-8 space-y-6">
            <h2 class="text-2xl font-black text-restall-dark dark:text-restall-light uppercase tracking-tight">
              {{ editingItem ? 'Edit Item' : 'Add Menu Item' }}
            </h2>

            <div v-if="formError" class="p-4 bg-rose-50 dark:bg-rose-500/10 border border-rose-200 dark:border-rose-500/20 rounded-2xl text-rose-500 text-sm font-bold">
              {{ formError }}
            </div>

            <form @submit.prevent="submitForm" class="space-y-4">
              <input
                  v-model="form.name"
                  type="text"
                  placeholder="Item name"
                  required
                  class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light"
              />
              <textarea
                  v-model="form.description"
                  placeholder="Description (optional)"
                  rows="2"
                  class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light resize-none"
              />
              <input
                  v-model.number="form.price"
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="Price"
                  required
                  class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light"
              />
              <select
                  v-model="form.menu_category_id"
                  required
                  class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light cursor-pointer"
              >
                <option :value="0" disabled>Select category</option>
                <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
              </select>
              <div class="flex gap-3 pt-2">
                <BaseButton type="button" variant="ghost" class="flex-1 rounded-2xl" @click="closeModal">Cancel</BaseButton>
                <BaseButton type="submit" variant="primary" class="flex-1 rounded-2xl" :disabled="submitting">
                  <Loader2 v-if="submitting" class="w-4 h-4 animate-spin mr-2" />
                  {{ editingItem ? 'Save Changes' : 'Add Item' }}
                </BaseButton>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { Plus, Edit, Trash2, Loader2 } from 'lucide-vue-next';
import { API } from '@/api';
import type { MenuItemData } from '@/api/menu.service';
import { useToast } from '@/composables/useToast';
import BaseButton from '@/components/UI/BaseButton.vue';

const { success, error: toastError } = useToast();

interface Category { id: number; name: string; items: RawItem[]; }
interface RawItem { id: number; name: string; description?: string; price: number | string; is_available: boolean; menu_category_id: number; }
interface FlatItem extends RawItem { category_name: string; }

const loading = ref(true);
const categories = ref<Category[]>([]);
const items = ref<FlatItem[]>([]);
const search = ref('');
const activeCategoryId = ref<number | null>(null);
const togglingId = ref<number | null>(null);
const modalOpen = ref(false);
const submitting = ref(false);
const formError = ref('');
const editingItem = ref<FlatItem | null>(null);

const form = reactive<MenuItemData>({ name: '', description: '', price: 0, menu_category_id: 0 });

const filteredItems = computed(() => {
  let result = items.value;
  if (activeCategoryId.value !== null) {
    result = result.filter(i => i.menu_category_id === activeCategoryId.value);
  }
  if (search.value.trim()) {
    const q = search.value.toLowerCase();
    result = result.filter(i => i.name.toLowerCase().includes(q) || i.description?.toLowerCase().includes(q));
  }
  return result;
});

const formatPrice = (value: number | string) =>
    Number(value).toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });

const loadMenu = async () => {
  loading.value = true;
  try {
    const { data } = await API.menu.getCategories();
    categories.value = data;
    items.value = data.flatMap((cat: Category) =>
        (cat.items || []).map((item: RawItem) => ({ ...item, category_name: cat.name }))
    );
  } finally {
    loading.value = false;
  }
};

const toggleAvailability = async (item: FlatItem) => {
  togglingId.value = item.id;
  try {
    await API.menu.toggleAvailability(item.id);
    item.is_available = !item.is_available;
    success(`Item marked as ${item.is_available ? 'available' : 'unavailable'}`);
  } catch {
    toastError('Failed to update availability');
  } finally {
    togglingId.value = null;
  }
};

const removeItem = async (id: number) => {
  if (!confirm('Delete this menu item?')) return;
  try {
    await API.menu.destroyItem(id);
    items.value = items.value.filter(i => i.id !== id);
    success('Item deleted');
  } catch {
    toastError('Failed to delete item');
  }
};

const openAddModal = () => {
  editingItem.value = null;
  Object.assign(form, { name: '', description: '', price: 0, menu_category_id: 0 });
  formError.value = '';
  modalOpen.value = true;
};

const openEditModal = (item: FlatItem) => {
  editingItem.value = item;
  Object.assign(form, {
    name: item.name,
    description: item.description || '',
    price: Number(item.price),
    menu_category_id: item.menu_category_id,
  });
  formError.value = '';
  modalOpen.value = true;
};

const closeModal = () => {
  modalOpen.value = false;
};

const submitForm = async () => {
  submitting.value = true;
  formError.value = '';
  try {
    if (editingItem.value) {
      const { data } = await API.menu.updateItem(editingItem.value.id, form);
      const idx = items.value.findIndex(i => i.id === editingItem.value!.id);
      if (idx !== -1) {
        const cat = categories.value.find(c => c.id === data.menu_category_id);
        items.value[idx] = { ...data, category_name: cat?.name || '' };
      }
      success('Item updated');
    } else {
      const { data } = await API.menu.storeItem(form);
      const cat = categories.value.find(c => c.id === data.menu_category_id);
      items.value.push({ ...data, category_name: cat?.name || '' });
      success('Item added');
    }
    modalOpen.value = false;
  } catch (e: any) {
    formError.value = e.response?.data?.message || 'Failed to save item';
  } finally {
    submitting.value = false;
  }
};

onMounted(loadMenu);
</script>
