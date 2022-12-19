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
  width: 35vw;
  height: 20vh;
  padding: 10px;
  resize: none;
  color: white;
  outline: none;
  border: 2px solid white;
  border-radius: 5px;
  background: rgba(0, 0, 0, 0.2);
}
</style>