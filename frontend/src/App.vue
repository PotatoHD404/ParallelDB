<template>
  <div :style="image" class="image">
    <img alt="PD logo" class="logo" src="~@/assets/db.svg">
    <my-input
        v-model="query.text"
        type="text"
        placeholder="Запрос"/>
    <input-list :queryList="queryList"/>

  </div>
</template>

<script lang="ts">
import { Query} from "@/types/query";
import InputList from "@/components/InputList.vue";
// import InputForm from "@/components/InputForm.vue";
import MyInput from "@/components/UI/MyInput.vue";

export default {
  components: {MyInput, InputList},
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
    }
  },
  methods: {
    // createQuery(query: Query) {
    //   this.queryList.push(query)
    // }
  }
};
</script>

<style>
.app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  background-repeat: no-repeat;
}

.image {
  background-repeat: no-repeat;
  background-image: url("~@/assets/gradient.svg");
}

.logo {
  position: absolute;
  top: 30px;
  left: 30px;
}
</style>
