import { createApp } from 'vue'
import App from './App.vue'
import MyButton from '@/components/UI/MyButton.vue'

const app = createApp(App)
app.component('MyButton', MyButton)
app.mount('#app')
