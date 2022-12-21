<template>
  <div class="toggle-btn" id="_1st-toggle-btn">
    <input type="checkbox">
    <span></span>
  </div>
</template>

<script>
export default {
  name: "my-checkbox",
}
</script>

<style scoped>
.toggle-btn {
  transform: scale(0.6);
  position: absolute;
  width: 145px;
  height: 74px;
  top: 35px;
  right: 20px;
  border-radius: 40px;
}

input[type="checkbox"] {
  width: 100%;
  height: 100%;
  position: absolute;
  cursor: pointer;
  opacity: 0;
  z-index: 2;
}

#_1st-toggle-btn span {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  overflow: hidden;
  opacity: 1;
  background: rgba(255, 255, 255, 1);
  border-radius: 40px;
  transition: 0.2s ease background-color, 0.2s ease opacity;
}

#_1st-toggle-btn span:before,
#_1st-toggle-btn span:after {
  content: "";
  position: absolute;
  top: 8px;
  width: 58px;
  height: 58px;
  border-radius: 50%;
  transition: 0.3s ease transform, 0.2s ease background-color;
}

#_1st-toggle-btn span:before {
  background-color: #fff;
  transform: translate(-58px, 0px);
  z-index: 1;
}

#_1st-toggle-btn span:after {
  background-color: #000;
  transform: translate(8px, 0px);
  z-index: 0;
}

#_1st-toggle-btn input[type="checkbox"]:checked + span {
  background-color: #000;
}

#_1st-toggle-btn input[type="checkbox"]:active + span {
  opacity: 0.5;
}

#_1st-toggle-btn input[type="checkbox"]:checked + span:before {
  background-color: #000;
  transform: translate(50px, -19px);
}

#_1st-toggle-btn input[type="checkbox"]:checked + span:after {
  background-color: #fff;
  transform: translate(79px, 0px);
}
</style>