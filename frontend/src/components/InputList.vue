<template>
  <div class="plist" v-if="queryList.length > 0">
    <div class="list" v-for="(query, index) in queryList" :key="index">
      <div> {{ query.text }}</div>
    </div>
  </div>
  <h4 v-else style="color: white">
    Очередь запросов пуста
  </h4>
</template>

<script lang="ts">
import {Query} from "@/types/query";
import {defineComponent} from "vue";

export default defineComponent({
  name: "input-list",
  props: {
    queryList: {
      type: Array as () => Query[],
      required: true,
    }
  }
});
</script>

<style scoped>
.plist {
  overflow-y: auto;
  max-height: 45vh;
  max-width: 50vw;
}

.list {
  margin-top: 15px;
  padding: 10px;
  color: white;
  border: 2px solid white;
  border-radius: 5px;
  left: 15px;
  background: rgba(0, 0, 0, 0.2);
}
</style>