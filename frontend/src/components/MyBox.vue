<template>
  <div>
    <div class="box  blackBox">
      <p class="graphs greenBox" v-html="graph" v-bind:class="{negative : isDark}"/>
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
  }
})
</script>

<style scoped>
.box {
  width: 45vw;
  height: 73vh;
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
  top: 10px;
  left: 10px;
  margin-bottom: 20px;
  overflow: auto;
  table-layout: fixed;
}
.negative {
  filter: invert(100%);
}
</style>