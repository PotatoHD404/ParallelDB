<template>
  <div class="my-warning alert redBox">
    <slot></slot>
  </div>
</template>

<script>
export default {
  name: "my-warning",
}
</script>

<style scoped>
.my-warning {
  font-family: Montserrat, sans-serif;
  padding: 15px;
  position: absolute;
  top: 40px;
  color: white;
  z-index: 1;
  width: 15%;
  text-align: center;
  transition: margin-right 2s ease-in-out;
  background-color: #ffd7d7;
}

.redBox {
  border-radius: 5px;
  background: rgba(255, 0, 0, 0.3);
  border: 2px solid white;
}
</style>