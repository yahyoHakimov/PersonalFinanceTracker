<template>
  <div id="app">
    <NavBar />
    <main class="container-fluid mt-4">
      <router-view />
    </main>
    <Toast />
  </div>
</template>

<script>
import NavBar from './components/NavBar.vue'
import Toast from './components/Toast.vue'

export default {
  name: 'App',
  components: {
    NavBar,
    Toast
  }
}
</script>