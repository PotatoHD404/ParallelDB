<template>
  <textarea :value="modelValue" @input="updateArea" class="textarea"></textarea>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: "my-textarea",
  props: {
    modelValue: {
      type: String,
    },
  },
  methods: {
    updateArea(e: any) {
      this.$emit("update:modelValue", e.target.value);
    },
  },
})
</script>

<style scoped>
.textarea {
  margin-top: 15px;
  padding: 10px;
  color: white;
  border: 1px solid white;
  border-radius: 5px;
  background: none;
}
</style>