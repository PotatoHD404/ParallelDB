<template>
  <div v-if="queryList.length > 0">
    <div class="list" v-for="(query, index) in queryList" :key="index">
      <div> {{ query.text }} </div>
    </div>
  </div>
  <h2 v-else>
    Очередь запросов пуста
  </h2>
</template>

<script lang="ts">
import {Query} from "@/types/query";

export default {
  name: "input-list",
  props: {
    queryList: {
      type: Array as () => Query[],
      required: true,
    }
  }
}
</script>

<style scoped>
.list {
  margin-top: 15px;
  padding: 10px;
  color: white;
  display: flex;
  border: 1px solid #ccc;
  border-radius: 5px;
  background: none;
}
</style>