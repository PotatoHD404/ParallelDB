<template>
  <form @submit.prevent>

    <div>
      <div class="box blackBox ">
        <p class="graphs" v-html="graph"/>
        <div class="panel" v-if="treeType !== 0">
          <my-button @click="changeTree(1)" class="buttons button1"> SyntaxTree </my-button>
          <my-button @click="changeTree(2)" class="buttons button2"> QueryTree </my-button>
          <my-button @click="changeTree(3)" class="buttons button3"> PlannerTree </my-button>
        </div>
      </div>
    </div>
  </form>
</template>

<script lang="ts">
import {defineComponent} from 'vue';
import MyButton from "@/components/UI/MyButton.vue";

export default defineComponent({
  name: "my-box",
  components: {MyButton},
  props: {
    graph: {
      type: String,
      required: true,
    },
    isDark: {
      type: Boolean,
      required: true,
    },
    treeType: {
      type: Number,
      required: true,
    },
  },
  methods: {
    changeTree: function (type: number) {
      this.$emit('changeTree', type)
    }
  }
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

.panel {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 10%;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
}

.buttons {
  position: absolute;
  padding: 15px;
  border-radius: 0;
  width: 32.5%;
  height: 80%;
  margin-bottom: 1.5%;
  background: transparent;
  border: none;
}
.buttons:hover {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  /*transform: perspective(1px) scale3d(1.044, 1.044, 1) translateZ(0) !important;*/
}

.button1 {
  left: 5px;
  transform: translateY(10%);
}

.button2 {
  transform: translateY(10%);
}

.button3 {
  right: 5px;
  transform: translateY(10%);
}

.choices {
  background: rgba(255, 255, 255, 0.1);
}

</style>
