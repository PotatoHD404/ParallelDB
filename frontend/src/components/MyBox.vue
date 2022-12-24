<template>
  <div>
    <div class="box  blackBox ">
      <p class="graphs" v-html="graph"/>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent} from 'vue';

export default defineComponent({
  name: "my-box",
  props: {
    graph: {
      type: String,
      required: true,
    },
    isDark: {
      type: Boolean,
      required: true,
    }
  },
})
</script>

<style scoped>
.box {
  width: 45vw;
  height: 76vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
.graphs {
  position: relative;
  width: 100%;
  height: 100%;
  object-fit: contain;
  overflow: hidden;
  table-layout: fixed;
}

</style>
