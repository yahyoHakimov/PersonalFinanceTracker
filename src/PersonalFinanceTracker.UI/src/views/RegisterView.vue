<template>
  <div class="row justify-content-center">
    <div class="col-md-6 col-lg-4">
      <div class="card">
        <div class="card-header text-center">
          <h4>Register</h4>
        </div>
        <div class="card-body">
          <form @submit.prevent="handleRegister">
            <div class="mb-3">
              <label for="username" class="form-label">Username</label>
              <input
                type="text"
                class="form-control"
                id="username"
                v-model="form.username"
                :class="{ 'is-invalid': errors.username }"
                required
              />
              <div v-if="errors.username" class="invalid-feedback">
                {{ errors.username }}
              </div>
            </div>
            
            <div class="mb-3">
              <label for="email" class="form-label">Email</label>
              <input
                type="email"
                class="form-control"
                id="email"
                v-model="form.email"
                :class="{ 'is-invalid': errors.email }"
                required
              />
              <div v-if="errors.email" class="invalid-feedback">
                {{ errors.email }}
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
            
            <div class="mb-3">
              <label for="confirmPassword" class="form-label">Confirm Password</label>
              <input
                type="password"
                class="form-control"
                id="confirmPassword"
                v-model="form.confirmPassword"
                :class="{ 'is-invalid': errors.confirmPassword }"
                required
              />
              <div v-if="errors.confirmPassword" class="invalid-feedback">
                {{ errors.confirmPassword }}
              </div>
            </div>
            
            <button
              type="submit"
              class="btn btn-primary w-100"
              :disabled="authStore.isLoading"
            >
              <span v-if="authStore.isLoading" class="loading-spinner me-2"></span>
              {{ authStore.isLoading ? 'Creating Account...' : 'Register' }}
            </button>
          </form>
          
          <div class="text-center mt-3">
            <p>Already have an account? <router-link to="/login">Login here</router-link></p>
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
  name: 'RegisterView',
  setup() {
    const router = useRouter()
    const authStore = useAuthStore()
    const toastStore = useToastStore()
    
    const form = reactive({
      username: '',
      email: '',
      password: '',
      confirmPassword: ''
    })
    
    const errors = ref({})
    
    const handleRegister = async () => {
      errors.value = {}
      
      // Basic validation
      if (!form.username) {
        errors.value.username = 'Username is required'
      }
      
      if (!form.email) {
        errors.value.email = 'Email is required'
      }
      
      if (!form.password) {
        errors.value.password = 'Password is required'
      } else if (form.password.length < 6) {
        errors.value.password = 'Password must be at least 6 characters'
      }
      
      if (!form.confirmPassword) {
        errors.value.confirmPassword = 'Confirm password is required'
      } else if (form.password !== form.confirmPassword) {
        errors.value.confirmPassword = 'Passwords do not match'
      }
      
      if (Object.keys(errors.value).length > 0) {
        return
      }
      
      const result = await authStore.register(form)
      
      if (result.success) {
        toastStore.success('Registration successful!')
        router.push('/')
      } else {
        toastStore.error(result.error || 'Registration failed')
      }
    }
    
    return {
      form,
      errors,
      authStore,
      handleRegister
    }
  }
}
</script>