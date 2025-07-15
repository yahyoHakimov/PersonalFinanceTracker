<template>
  <div class="row justify-content-center">
    <div class="col-md-6 col-lg-4">
      <div class="card">
        <div class="card-header text-center">
          <h4>Login</h4>
        </div>
        <div class="card-body">
          <form @submit.prevent="handleLogin">
            <div class="mb-3">
              <label for="usernameOrEmail" class="form-label">Username or Email</label>
              <input
                type="text"
                class="form-control"
                id="usernameOrEmail"
                v-model="form.usernameOrEmail"
                :class="{ 'is-invalid': errors.usernameOrEmail }"
                required
              />
              <div v-if="errors.usernameOrEmail" class="invalid-feedback">
                {{ errors.usernameOrEmail }}
              </div>
            </div>
            
            <div class="mb-3">
              <label for="password" class="form-label">Password</label>
              <input
                type="password"
                class="form-control"
                id="password"
                v-model="form.password"
                :class="{ 'is-invalid': errors.password }"
                required
              />
              <div v-if="errors.password" class="invalid-feedback">
                {{ errors.password }}
              </div>
            </div>
            
            <div class="mb-3 form-check">
              <input
                type="checkbox"
                class="form-check-input"
                id="rememberMe"
                v-model="form.rememberMe"
              />
              <label class="form-check-label" for="rememberMe">
                Remember me
              </label>
            </div>
            
            <button
              type="submit"
              class="btn btn-primary w-100"
              :disabled="authStore.isLoading"
            >
              <span v-if="authStore.isLoading" class="loading-spinner me-2"></span>
              {{ authStore.isLoading ? 'Logging in...' : 'Login' }}
            </button>
          </form>
          
          <div class="text-center mt-3">
            <p>Don't have an account? <router-link to="/register">Register here</router-link></p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

export default {
  name: 'LoginView',
  setup() {
    const router = useRouter()
    const authStore = useAuthStore()
    const toastStore = useToastStore()
    
    const form = reactive({
      usernameOrEmail: '',
      password: '',
      rememberMe: false
    })
    
    const errors = ref({})
    
    const handleLogin = async () => {
      errors.value = {}
      
      // Basic validation
      if (!form.usernameOrEmail) {
        errors.value.usernameOrEmail = 'Username or email is required'
      }
      
      if (!form.password) {
        errors.value.password = 'Password is required'
      }
      
      if (Object.keys(errors.value).length > 0) {
        return
      }
      
      const result = await authStore.login(form)
      
      if (result.success) {
        toastStore.success('Login successful!')
        router.push('/')
      } else {
        toastStore.error(result.error || 'Login failed')
      }
    }
    
    return {
      form,
      errors,
      authStore,
      handleLogin
    }
  }
}
</script>