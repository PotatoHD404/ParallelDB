<template>
  <div class="my-table blackBox">
    <table>
      <thead>
      <tr>
        <th>
          База данных
        </th>

        <th>
          Таблица
        </th>

        <!--      <th v-for="column in columns" :key="column">-->
        <!--        {{ column }}-->
        <!--      </th>-->
      </tr>
      </thead>
      <tbody>
      <tr>
        <td>
          дата 1
        </td>
        <td>
          дата 2
        </td>
        <!--      <td>-->
        <!--        {{ row.database }}-->
        <!--      </td>-->
        <!--      <td>-->
        <!--        {{ row.table }}-->
        <!--      </td>-->
        <!--      <td v-for="column in columns" :key="column">-->
        <!--        {{ row[column] }}-->
        <!--      </td>-->
      </tr>
      <tr>
        <td>
          дата 1
        </td>
        <td>
          дата 2
        </td>
      </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  name: "my-table"
}
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