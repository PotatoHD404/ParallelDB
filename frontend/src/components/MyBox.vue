<template>
  <div>
    <div class="box  blackBox">
      <p class="greenBox" v-html="graph"/>
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
/*.test {*/
/*  !*put it in left upper corner*!*/
/*  position: absolute;*/
/*  top: 0;*/
/*  left: 0;*/
/*  overflow: hidden;*/
/*  table-layout: fixed;*/
/*}*/
</style>