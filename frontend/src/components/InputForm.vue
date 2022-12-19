<template>
  <form @submit.prevent>
    <my-textarea
        v-model="query.text"
        type="text"
        placeholder="|"
    />
      <my-button @click="createQuery"> Создать </my-button>
  </form>
</template>

<script lang="ts">
import type { Query } from '@/types/query';
import MyTextarea from "@/components/UI/MyTextarea.vue";
import MyButton from "@/components/UI/MyButton.vue";
import { defineComponent } from 'vue';

export default defineComponent({
  name: "input-form",
  components: {MyTextarea, MyButton},
  data() {
    return {
      query: {
        text: '',
      } as Query,
    }
  },
  methods: {
    createQuery: function () {
      this.$emit('create', this.query)
      this.query = {
        text: '',
      }
    }
  }
})
</script>

<style scoped>

</style>