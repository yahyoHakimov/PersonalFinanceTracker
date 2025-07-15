<template>
  <nav class="navbar navbar-expand-lg navbar-light bg-white shadow-sm">
    <div class="container">
      <router-link class="navbar-brand" to="/">
        <i class="bi bi-wallet2"></i>
        Personal Finance Tracker
      </router-link>
      
      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarNav"
      >
        <span class="navbar-toggler-icon"></span>
      </button>
      
      <div class="collapse navbar-collapse" id="navbarNav">
        <ul class="navbar-nav me-auto" v-if="authStore.isAuthenticated">
          <li class="nav-item">
            <router-link class="nav-link" to="/">
              <i class="bi bi-house-door"></i>
              Dashboard
            </router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" to="/transactions">
              <i class="bi bi-arrow-left-right"></i>
              Transactions
            </router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" to="/categories">
              <i class="bi bi-tags"></i>
              Categories
            </router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" to="/reports">
              <i class="bi bi-bar-chart"></i>
              Reports
            </router-link>
          </li>
        </ul>
        
        <ul class="navbar-nav">
          <li class="nav-item dropdown" v-if="authStore.isAuthenticated">
            <a
              class="nav-link dropdown-toggle"
              href="#"
              role="button"
              data-bs-toggle="dropdown"
            >
              <i class="bi bi-person-circle"></i>
              {{ authStore.user?.username || 'User' }}
            </a>
            <ul class="dropdown-menu">
              <li>
                <router-link class="dropdown-item" to="/profile">
                  <i class="bi bi-person"></i>
                  Profile
                </router-link>
              </li>
              <li><hr class="dropdown-divider"></li>
              <li>
                <a class="dropdown-item" href="#" @click="handleLogout">
                  <i class="bi bi-box-arrow-right"></i>
                  Logout
                </a>
              </li>
            </ul>
          </li>
          
          <template v-else>
            <li class="nav-item">
              <router-link class="nav-link" to="/login">Login</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/register">Register</router-link>
            </li>
          </template>
        </ul>
      </div>
    </div>
  </nav>
</template>

<script>
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

export default {
  name: 'NavBar',
  setup() {
    const authStore = useAuthStore()
    const router = useRouter()
    
    const handleLogout = async () => {
      await authStore.logout()
      router.push('/login')
    }
    
    return {
      authStore,
      handleLogout
    }
  }
}
</script>