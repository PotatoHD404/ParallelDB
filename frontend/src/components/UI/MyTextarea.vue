<template>
  <textarea :value="modelValue" @input="updateArea" class="textarea blackBox"></textarea>
</template>

<script lang="ts">
import {defineComponent} from 'vue';

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
  width: 45vw;
  height: 20vh;
  padding: 10px;
  resize: none;
  color: white;
  outline: none;
}
</style>