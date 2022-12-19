<template>
  <div>
    <div class="box">
    </div>
  </div>
</template>

<script>
export default {
  name: "my-box",
}
</script>

<style scoped>
.box {
  width: 55vw;
  height: 70vh;
  background: rgba(0, 0, 0, 0.2);
  border-radius: 5px;
  border: 2px solid white;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
</style>