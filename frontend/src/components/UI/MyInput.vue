<template>
  <input :value="modelValue" @input="updateInput" class="input" type="text">
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: "my-input",
  props: {
    modelValue: {
      type: String,
    },
  },
  methods: {
    updateInput(e: any) {
      this.$emit("update:modelValue", e.target.value);
    },
  },
})
</script>

<style scoped>
.input {
  margin-top: 15px;
  padding: 10px;
  color: white;
  border: 1px solid #ccc;
  border-radius: 5px;
  background: none;
}
</style>