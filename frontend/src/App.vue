<template>
  <div :style="image" class="image" v-bind:class="{theme : isDark}">
    <my-warning v-if="error !== ''">{{error}} </my-warning>
    <my-checkbox @change="changeTheme" v-bind:class="{negative : isDark === false}"/>
    <img alt="PD logo" class="logo" src="~@/assets/db.svg">
<!--    <input-list :queryList="queryList" v-bind:class="{negative : isDark === false}"/>-->
    <input-form style="position: absolute; left: 40px; top: 18vh" @create="createQuery"/>
    <my-box style="position: absolute; right: 40px; top: 18vh" :graph="graph" :isDark="isDark"/>
    <my-table style="position: absolute; left: 40px; top: 49vh" :headers="headers" :rows="rows"/>
    <h4 class="sign">by Kornachyk M.V & Lukichev A.N</h4>
    <!--    <p v-html="graph"/>-->
  </div>
</template>

<script lang="ts">
import {defineComponent} from 'vue';
import {Query} from "@/types/query";
import {Response} from "@/types/response";
import Viz from 'viz.js';
import {Module, render} from 'viz.js/full.render.js';
// import InputList from "@/components/InputList.vue";
import InputForm from "@/components/InputForm.vue";
import MyBox from "@/components/MyBox.vue";
import MyTable from "@/components/UI/MyTable.vue";
import MyCheckbox from "@/components/UI/MyCheckbox.vue";
import MyWarning from "@/components/UI/MyWarning.vue";
import svgPanZoom from "svg-pan-zoom";

export default defineComponent({
  components: {MyTable, MyBox, MyCheckbox, InputForm, MyWarning},

  data() {
    return {
      image: {
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        height: '100vh',
        width: '100vw',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center'
      },
      query: {
        text: '',
      } as Query,
      queryList: [{text: 'SELECT * FROM users'},
        {text: 'SELECT * FROM users WHERE id = 1'}] as Query[],
      isDark: localStorage.getItem("theme") === "false",
      graph: '',
      headers: [] as string[],
      rows: [] as any[][],
      error: '',
    }
  },
  methods: {
    async createQuery(query: Query) {
      if (query.text.trim()) {
        // this.queryList.push(query) <--- this can be used to store queries
        let myHeaders = new Headers();
        myHeaders.append("Content-Type", "application/json");

        let raw = JSON.stringify({
          "request": query.text
        });

        let requestOptions = {
          method: 'POST',
          headers: myHeaders,
          body: raw,
        };

        let result = await fetch("http://localhost:6386/", requestOptions);

        if (result.status != 200) {
          this.showError('Something went wrong!')
        }

        let data:Response = await result.json();
        let dot = data.syntaxTree;
        this.headers = data.columns;
        this.rows = data.rows;
        this.renderGraph(dot);
      } else {
        this.showError('Empty query!')
      }
    },
    changeTheme() {
      localStorage.setItem("theme", this.isDark.toString());
      this.isDark = !this.isDark;
    },
    renderGraph(dot: string) {
      let viz = new Viz({Module, render});
      viz.renderString(dot)
          .then(element => {
            element = element.replace('<svg ', '<svg id="graph" class="graphs" style="filter: invert(100%);" ');
            this.graph = element;
            this.$nextTick(() => {
              svgPanZoom('#graph', {
                maxZoom: 20,
                zoomEnabled: true,
                controlIconsEnabled: true,
              });
            })
          })
          .catch(() => {
            this.showError('Something went wrong!')
            viz = new Viz({Module, render});
          });
    },
    showError(error: string) {
      this.error = error;
      setTimeout(() => {
        this.error = '';
      }, 3000)
    }

  },
  setup() {
    if (localStorage.getItem("theme") === "") {
      localStorage.setItem("theme", "true");
    }
  },
});
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

.theme {
  filter: invert(100%);
}

.image {
  font-family: Montserrat, sans-serif;
  background-repeat: no-repeat;
  background-image: url("~@/assets/gradient.svg");
  display: flex;
  flex-direction: column;
}

.logo {
  position: absolute;
  top: 20px;
  left: 30px;
}

.sign {
  position: absolute;
  bottom: 10px;
  right: 40px;
  color: white;
  font-size: 20px;
}

.greenBox {
  background-color: rgba(0, 255, 0, 0.2) !important;
}

.blackBox {
  border-radius: 5px;
  background: rgba(0, 0, 0, 0.5);
  border: 2px solid white;
}

.graphs {
  width: 100%;
  height: 100%;
}

.negative {
  filter: invert(100%);
}


::placeholder {
  color: #D3D3D3;
}
</style>
