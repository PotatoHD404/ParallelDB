<template>
  <div class="my-table blackBox">
    <table>
      <thead>
        <tr>
          <th v-for="header in headers" :key="header">
            {{ header }}
          </th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, index) in rows" :key="index">
          <td v-for="(cell, index) in row" :key="index">
            {{ cell }}
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
export default defineComponent({
  name: "my-table",
  props: {
    headers: {
      type: Array as () => string[],
      required: true,
    },
    rows: {
      type: Array as () => any[][],
      required: true,
    },
  },
});
</script>

<style scoped>
table{
  overflow: hidden;
  table-layout: fixed;
}
th{
  padding: 20px 15px;
  background-color: rgba(255,255,255,0.05);
  text-align: left;
  font-weight: 500;
  font-size: 12px;
  color: white;
  overflow-x: auto;
  text-transform: uppercase;
}
td{
  padding: 15px;
  text-align: left;
  vertical-align:middle;
  font-weight: 300;
  font-size: 12px;
  color: white;
  overflow: hidden;
  border-bottom: solid 1px rgba(255,255,255,0.1);
}
.my-table {
  overflow-y: auto;
  height: 45vh;
  width: 45vw;
}

::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
::-webkit-scrollbar-track {
  -webkit-box-shadow: inset 0 0 6px rgba(255,255,255,0.3);
}
::-webkit-scrollbar-thumb {
  -webkit-box-shadow: inset 0 0 6px rgba(255,255,255,255.3);
}
</style>