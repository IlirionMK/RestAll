<script setup lang="ts">
import { onMounted } from 'vue';
import { echo } from './echo';

onMounted(() => {
  echo.channel('test-channel')
      .listen('.test.event', (data: any) => {
        console.log('Socket received:', data.content);
        alert('Backend message: ' + data.content);
      });
});
</script>