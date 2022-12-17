<template>
  <button class="btn">
    <slot></slot>
  </button>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: "my-button"
})
</script>

<style scoped>
.btn {
  margin-top: 15px;
  padding: 10px;
  color: white;
  border: 1px solid #ccc;
  border-radius: 5px;
  background: none;
}
</style>