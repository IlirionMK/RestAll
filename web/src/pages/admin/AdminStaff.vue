<template>
  <div class="space-y-10 font-sans">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <h1 class="text-4xl font-black text-restall-dark dark:text-restall-light tracking-tighter uppercase">
        Staff Management
      </h1>
      <BaseButton variant="primary" @click="modalOpen = true">
        <Plus class="w-5 h-5 mr-2" />
        Add Member
      </BaseButton>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
      <Loader2 class="w-10 h-10 animate-spin text-restall-gold" />
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
          v-for="member in staff"
          :key="member.id"
          class="bg-white dark:bg-gray-800 p-6 rounded-squircle shadow-sm border border-gray-100 dark:border-gray-700 flex flex-col items-center text-center"
      >
        <div class="w-20 h-20 rounded-2xl bg-restall-green/10 flex items-center justify-center text-restall-green font-black text-2xl mb-4 uppercase">
          {{ member.name.charAt(0) }}
        </div>
        <h3 class="font-bold text-xl text-restall-dark dark:text-restall-light">{{ member.name }}</h3>
        <p class="text-gray-500 text-sm font-medium mt-1 mb-4">{{ member.email }}</p>
        <BaseBadge :variant="roleBadge(member.role)" class="mb-6 capitalize">{{ member.role }}</BaseBadge>

        <div class="w-full space-y-2 mt-auto">
          <select
              :value="member.role"
              @change="changeRole(member.id, ($event.target as HTMLSelectElement).value)"
              class="w-full px-3 py-2 bg-gray-50 dark:bg-gray-900 border border-gray-100 dark:border-gray-700 rounded-xl text-sm font-bold text-restall-dark dark:text-restall-light outline-none cursor-pointer"
          >
            <option v-for="role in roles" :key="role" :value="role" class="capitalize">{{ role }}</option>
          </select>
          <BaseButton variant="ghost" size="sm" class="w-full rounded-xl text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-500/10" @click="removeMember(member.id)">
            Remove
          </BaseButton>
        </div>
      </div>
    </div>

    <Teleport to="body">
      <Transition enter-active-class="transition duration-200 ease-out" enter-from-class="opacity-0" enter-to-class="opacity-100" leave-active-class="transition duration-150" leave-from-class="opacity-100" leave-to-class="opacity-0">
        <div v-if="modalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4" @click.self="modalOpen = false">
          <div class="absolute inset-0 bg-black/50 backdrop-blur-sm" @click="modalOpen = false" />
          <div class="relative w-full max-w-md bg-white dark:bg-gray-900 rounded-[2.5rem] shadow-2xl p-8 space-y-6">
            <h2 class="text-2xl font-black text-restall-dark dark:text-restall-light uppercase tracking-tight">Add Staff Member</h2>

            <div v-if="formError" class="p-4 bg-rose-50 dark:bg-rose-500/10 border border-rose-200 dark:border-rose-500/20 rounded-2xl text-rose-500 text-sm font-bold">
              {{ formError }}
            </div>

            <form @submit.prevent="submitForm" class="space-y-4">
              <input v-model="form.name" type="text" placeholder="Full Name" required class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light" />
              <input v-model="form.email" type="email" placeholder="Email" required class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light" />
              <input v-model="form.password" type="password" placeholder="Password" required class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light" />
              <select v-model="form.role" required class="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800 border border-gray-100 dark:border-gray-700 rounded-2xl outline-none font-bold text-restall-dark dark:text-restall-light cursor-pointer">
                <option value="" disabled>Select role</option>
                <option v-for="role in roles" :key="role" :value="role" class="capitalize">{{ role }}</option>
              </select>
              <div class="flex gap-3 pt-2">
                <BaseButton type="button" variant="ghost" class="flex-1 rounded-2xl" @click="modalOpen = false">Cancel</BaseButton>
                <BaseButton type="submit" variant="primary" class="flex-1 rounded-2xl" :disabled="submitting">
                  <Loader2 v-if="submitting" class="w-4 h-4 animate-spin mr-2" />
                  Add Member
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
import { ref, reactive, onMounted } from 'vue';
import { Plus, Loader2 } from 'lucide-vue-next';
import { API } from '@/api';
import type { StaffMember } from '@/api/staff.service';
import { useToast } from '@/composables/useToast';
import BaseButton from '@/components/UI/BaseButton.vue';
import BaseBadge from '@/components/UI/BaseBadge.vue';

const { success, error: toastError } = useToast();

const loading = ref(true);
const staff = ref<StaffMember[]>([]);
const modalOpen = ref(false);
const submitting = ref(false);
const formError = ref('');

const roles = ['admin', 'cashier', 'chef', 'waiter'];

const form = reactive({ name: '', email: '', password: '', role: '' });

const roleBadge = (role: string) => {
  const map: Record<string, string> = { admin: 'dark', chef: 'green', cashier: 'gold' };
  return map[role] || 'default';
};

const loadStaff = async () => {
  loading.value = true;
  try {
    const { data } = await API.staff.index();
    staff.value = data;
  } finally {
    loading.value = false;
  }
};

const changeRole = async (id: number, role: string) => {
  try {
    await API.staff.updateRole(id, role);
    const member = staff.value.find(m => m.id === id);
    if (member) member.role = role;
    success('Role updated');
  } catch {
    toastError('Failed to update role');
  }
};

const removeMember = async (id: number) => {
  if (!confirm('Remove this staff member?')) return;
  try {
    await API.staff.destroy(id);
    staff.value = staff.value.filter(m => m.id !== id);
    success('Member removed');
  } catch {
    toastError('Failed to remove member');
  }
};

const submitForm = async () => {
  submitting.value = true;
  formError.value = '';
  try {
    const { data } = await API.staff.store(form);
    staff.value.push(data);
    modalOpen.value = false;
    Object.assign(form, { name: '', email: '', password: '', role: '' });
    success('Staff member added');
  } catch (e: any) {
    formError.value = e.response?.data?.message || 'Failed to add member';
  } finally {
    submitting.value = false;
  }
};

onMounted(loadStaff);
</script>
