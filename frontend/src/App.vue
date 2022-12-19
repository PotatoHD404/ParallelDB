<template>
  <div :style="image" class="image">
    <img alt="PD logo" class="logo" src="~@/assets/db.svg">
    <h4 class="sign">by Kornachyk M.V & Lukichev A.N</h4>>
    <input-form @create="createQuery"/>
    <input-list :queryList="queryList"/>
  </div>
</template>

<script lang="ts">
import { Query} from "@/types/query";
import InputList from "@/components/InputList.vue";
import InputForm from "@/components/InputForm.vue";
import { defineComponent } from 'vue';

export default defineComponent({
  components: {InputList, InputForm},
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
    createQuery(query: Query) {
      this.queryList.push(query)
    }
  }
});
</script>

<style>
.app {
  font-family: Montserrat, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
}

.image {
  font-family: Montserrat, sans-serif;
  background-repeat: no-repeat;
  background-image: url("~@/assets/gradient.svg");
}

.logo {
  position: absolute;
  top: 30px;
  left: 30px;
}

.sign {
  position: absolute;
  bottom: 5px;
  right: 30px;
  color: white;
  font-size: 20px;
}
</style>
