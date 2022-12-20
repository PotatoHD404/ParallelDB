<template>
  <div :style="image" class="image">
    <img alt="PD logo" class="logo" src="~@/assets/db.svg">
    <h4 class="sign">by Kornachyk M.V & Lukichev A.N</h4>
    <input-form style="position: absolute; left: 40px; top: 20vh" @create="createQuery"/>
    <input-list style="position: absolute; z-index:500; right: 75px; top: 22vh" :queryList="queryList"/>
    <my-box style="position: absolute; right: 40px; top: 20vh"/>
    <my-table style="position: absolute; left: 40px; top: 48vh"/>
  </div>
</template>

<script lang="ts">
import { Query} from "@/types/query";
import InputList from "@/components/InputList.vue";
import InputForm from "@/components/InputForm.vue";
import MyBox from "@/components/MyBox.vue";
import { defineComponent } from 'vue';
import MyTable from "@/components/UI/MyTable.vue";

export default defineComponent({
  components: {MyTable, MyBox, InputList, InputForm},
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
      if (query.text.trim()) {
        this.queryList.push(query)
      } else {
        alert('Запрос не может быть пустым')
      }
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
  display: flex;

  flex-direction: column;
}

.logo {
  position: absolute;
  top: 20px;
  /*width: 15vw;*/
  /*height: 10vh;*/
  left: 30px;
}
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

.sign {
  position: absolute;
  bottom: 20px;
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

</style>
