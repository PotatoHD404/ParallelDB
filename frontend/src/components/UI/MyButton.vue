<template>
  <button class="btn  blackBox">
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
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.7rem 2rem;
  font-family: "Poppins", sans-serif;
  font-weight: 700;
  text-align: center;
  text-decoration: none;
  color: #fff;
  backface-visibility: hidden;
  flex-direction: column;
  transition: transform 0.2s cubic-bezier(0.235, 0, 0.05, 0.95);
}

.btn:hover {
  transform: perspective(1px) scale3d(1.044, 1.044, 1) translateZ(0) !important;
}

</style>