<template>
  <form @submit.prevent>
    <h4> Создание запроса </h4>
    <my-input
        v-model="query.text"
        type="text"
        placeholder="Запрос"
    />
      <my-button @click="createQuery"> Создать </my-button>
  </form>
</template>

<script lang="ts">
import type { Query } from '@/types/query';
import MyInput from "@/components/UI/MyInput.vue";
import MyButton from "@/components/UI/MyButton.vue";

export default {
  name: "input-form",
  components: {MyInput, MyButton},
  data() {
    return {
      query: {
        text: '',
      } as Query,
    }
  },
  methods: {
    // createQuery() {
    //   this.query.id = Date.now();
    //   this.$emit('create', this.query)
    //   this.query = {
    //     text: '',
    //   }
    // }
  }
}
</script>

<style scoped>

</style>