<template>
  <div :style="image" class="image" v-bind:class="{theme : isDark}">
    <my-checkbox @change="changeTheme"/>
    <img alt="PD logo" class="logo" src="~@/assets/db.svg">
    <input-form style="position: absolute; left: 40px; top: 18vh" @create="createQuery"/>
    <input-list style="position: absolute; z-index:500; right: 75px; top: 22vh" :queryList="queryList"/>
    <my-box style="position: absolute; right: 40px; top: 18vh" :graph="graph" :isDark="isDark"/>
    <my-table style="position: absolute; left: 40px; top: 49vh"/>
    <h4 class="sign">by Kornachyk M.V & Lukichev A.N</h4>
<!--    <p v-html="graph"/>-->
  </div>
</template>

<script lang="ts">
import {defineComponent} from 'vue';
import {Query} from "@/types/query";
import Viz from 'viz.js';
import { Module, render } from 'viz.js/full.render.js';
import InputList from "@/components/InputList.vue";
import InputForm from "@/components/InputForm.vue";
import MyBox from "@/components/MyBox.vue";
import MyTable from "@/components/UI/MyTable.vue";
import MyCheckbox from "@/components/UI/MyCheckbox.vue";

export default defineComponent({
  components: {MyTable, MyBox, MyCheckbox, InputList, InputForm},

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
    }
  },
  methods: {
    createQuery(query: Query) {
      if (query.text.trim()) {
        this.queryList.push(query)
      } else {
        alert('Запрос не может быть пустым')
        let viz = new Viz({ Module, render });
        let dot = 'graph { a -- { b c d };\n' +
            '    b -- { c e };\n' +
            '    c -- { e f };\n' +
            '    d -- { f g };\n' +
            '    e -- h;\n' +
            '    f -- { h i j g };\n' +
            '    g -- k;\n' +
            '    h -- { o l };\n' +
            '    i -- { l m j };\n' +
            '    j -- { m n k };\n' +
            '    k -- { n r };\n' +
            '    l -- { o m };\n' +
            '    m -- { o p n };\n' +
            '    n -- { q r };\n' +
            '    o -- { s p };\n' +
            '    p -- { s t q };\n' +
            '    q -- { t r };\n' +
            '    r -- t;\n' +
            '    s -- z;\n' +
            '    t -- z;}';
        viz.renderString(dot)
            .then(result => {
              this.graph = result;
            })
            .catch(error => {
              // Create a new Viz instance (@see Caveats page for more info)
              viz = new Viz({ Module, render });
              // Possibly display the error
              console.error(error);
            });
      }
    },
    changeTheme() {
      localStorage.setItem("theme", this.isDark.toString());
      this.isDark = !this.isDark
    },
    // renderGraph() {
    //   let viz = new Viz({ Module, render });
    //   let dot = 'digraph { a -> b -> c -> d -> e; }';
    //   viz.renderString(dot)
    //       .then(result => {
    //         this.graph = result;
    //       })
    //       .catch(error => {
    //         // Create a new Viz instance (@see Caveats page for more info)
    //         viz = new Viz({ Module, render });
    //         // Possibly display the error
    //         console.error(error);
    //       });
    // }
  }
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

::placeholder {
  color:    #D3D3D3;
}
</style>
