import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import apiClient from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(localStorage.getItem('token') || null)
  const refreshToken = ref(localStorage.getItem('refreshToken') || null)
  const isLoading = ref(false)

  const isAuthenticated = computed(() => !!token.value)

  const login = async (credentials) => {
    try {
      isLoading.value = true
      console.log('Sending login request...', credentials)
      
      const response = await apiClient.post('/auth/login', credentials)
      
      console.log('Full response:', response)
      console.log('Response data:', response.data)
    
      if (response.data.success) {
        // Your API returns accessToken and refreshToken, not the property names expected
        const authData = response.data.data
        const accessToken = authData.accessToken
        const newRefreshToken = authData.refreshToken
        const userData = authData.user
        
        token.value = accessToken
        refreshToken.value = newRefreshToken
        user.value = userData
        
        localStorage.setItem('token', accessToken)
        localStorage.setItem('refreshToken', newRefreshToken)
        
        // Set default authorization header
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`
        
        return { success: true }
      }
      
      return { success: false, error: response.data.message }
    } catch (error) {
      console.error('Login error details:', {
        message: error.message,
        response: error.response,
        status: error.response?.status,
        data: error.response?.data
      })
      return { success: false, error: error.response?.data?.message || 'Login failed' }
    } finally {
      isLoading.value = false
    }
  }

  const register = async (userData) => {
    try {
      isLoading.value = true
      const response = await apiClient.post('/auth/register', userData)
      
      if (response.data.success) {
        const authData = response.data.data
        const accessToken = authData.accessToken
        const newRefreshToken = authData.refreshToken
        const newUser = authData.user
        
        token.value = accessToken
        refreshToken.value = newRefreshToken
        user.value = newUser
        
        localStorage.setItem('token', accessToken)
        localStorage.setItem('refreshToken', newRefreshToken)
        
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`
        
        return { success: true }
      }
      
      return { success: false, error: response.data.message }
    } catch (error) {
      return { success: false, error: error.response?.data?.message || 'Registration failed' }
    } finally {
      isLoading.value = false
    }
  }

  const logout = async () => {
    try {
      await apiClient.post('/auth/logout')
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      token.value = null
      refreshToken.value = null
      user.value = null
      
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      
      delete apiClient.defaults.headers.common['Authorization']
    }
  }

  const getCurrentUser = async () => {
    try {
      const response = await apiClient.get('/auth/me')
      if (response.data.success) {
        user.value = response.data.data
      }
    } catch (error) {
      console.error('Get current user error:', error)
      await logout()
    }
  }

  const refreshAccessToken = async () => {
    try {
      const response = await apiClient.post('/auth/refresh', {
        accessToken: token.value,
        refreshToken: refreshToken.value
      })
      
      if (response.data.success) {
        const authData = response.data.data
        const newAccessToken = authData.accessToken
        const newRefreshToken = authData.refreshToken
        
        token.value = newAccessToken
        refreshToken.value = newRefreshToken
        
        localStorage.setItem('token', newAccessToken)
        localStorage.setItem('refreshToken', newRefreshToken)
        
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${newAccessToken}`
        
        return true
      }
      
      return false
    } catch (error) {
      console.error('Token refresh error:', error)
      await logout()
      return false
    }
  }

  // Initialize auth state
  if (token.value) {
    apiClient.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
    getCurrentUser()
  }

  return {
    user,
    token,
    refreshToken,
    isLoading,
    isAuthenticated,
    login,
    register,
    logout,
    getCurrentUser,
    refreshAccessToken
  }
})
