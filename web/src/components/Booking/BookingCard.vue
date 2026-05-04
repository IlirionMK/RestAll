<template>
  <div class="group bg-white dark:bg-gray-800/40 p-8 rounded-[3rem] border border-gray-100 dark:border-gray-700 hover:shadow-2xl transition-all duration-500 flex flex-col justify-between">
    <div class="space-y-8 text-left">
      <div class="flex justify-between items-center">
        <div :class="['px-5 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest', statusStyles]">
          {{ booking.status }}
        </div>
        <span class="text-[10px] font-black text-gray-300 tracking-widest uppercase">ID-{{ booking.id }}</span>
      </div>

      <div class="space-y-2">
        <h4 class="text-3xl font-black text-restall-dark dark:text-restall-light uppercase tracking-tighter truncate">
          {{ booking.restaurant?.name || 'RestAll Premium' }}
        </h4>
        <div class="flex items-center text-gray-400 font-bold text-xs uppercase tracking-tight">
          <MapPin class="w-4 h-4 mr-2 text-restall-green" />
          {{ t('bookings.location_placeholder') }}
        </div>
      </div>

      <div class="grid grid-cols-2 gap-6 py-6 border-y border-gray-50 dark:border-gray-700/50">
        <div class="space-y-1">
          <span class="text-[10px] font-black text-gray-400 uppercase tracking-widest">{{ t('bookings.date_label') }}</span>
          <div class="text-sm font-black text-restall-dark dark:text-restall-light flex items-center">
            <Clock class="w-4 h-4 mr-2 text-restall-gold" /> {{ formattedDate }}
          </div>
        </div>
        <div class="space-y-1">
          <span class="text-[10px] font-black text-gray-400 uppercase tracking-widest">{{ t('bookings.table_label') }}</span>
          <div class="text-sm font-black text-restall-dark dark:text-restall-light flex items-center">
            <Users class="w-4 h-4 mr-2 text-restall-green" /> №{{ booking.table?.number }}
          </div>
        </div>
      </div>
    </div>

    <div v-if="$slots.actions" class="mt-8">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { MapPin, Clock, Users } from 'lucide-vue-next';

const { t } = useI18n();
const props = defineProps<{
  booking: any
}>();

const formattedDate = computed(() => {
  return new Date(props.booking.reservation_time).toLocaleString('en-GB', {
    day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit'
  });
});

const statusStyles = computed(() => {
  const styles: Record<string, string> = {
    confirmed: 'bg-restall-green/10 text-restall-green',
    pending: 'bg-restall-gold/10 text-restall-gold',
    cancelled: 'bg-rose-100 text-rose-500',
    completed: 'bg-gray-100 text-gray-500',
  };
  return styles[props.booking.status] || 'bg-gray-100 text-gray-400';
});
</script>