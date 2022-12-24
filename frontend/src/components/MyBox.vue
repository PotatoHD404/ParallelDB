<template>
  <div>
    <div class="box  blackBox">
      <p class="graphs greenBox" v-html="svg" v-bind:class="{negative : isDark}"/>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent} from 'vue';
import svgPanZoom from 'svg-pan-zoom';
import Viz from 'viz.js';
import { Module, render } from 'viz.js/full.render.js';

export default defineComponent({
  name: "my-box",
  data() {
    return {
      svg: ''
    }
  },
  methods: {
    loadGraph() {
      let viz = new Viz({ Module, render });
      viz.renderString("digraph { a -> b; }")
          .then(element => {
            element = element.replace('<svg ', '<svg id="graph" class="w-full h-full" ');
            this.svg = element;
            this.$nextTick(() => {
              svgPanZoom('#graph', {
                maxZoom: 20,
                zoomEnabled: true,
                controlIconsEnabled: true,
              });
            })
          })
          .catch(() => {
            // Create a new Viz instance (@see Caveats page for more info)
            viz = new Viz({ Module, render });
          });
    }
  },
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
  activated() {
    this.loadGraph();
  },
  mounted() {
    this.loadGraph();
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
