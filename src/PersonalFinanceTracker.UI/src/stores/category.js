import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/api'

export const useCategoryStore = defineStore('category', () => {
  const categories = ref([])
  const isLoading = ref(false)
  const pagination = ref({
    page: 1,
    limit: 10,
    total: 0,
    totalPages: 0
  })

  const getCategories = async (params = {}) => {
    try {
      isLoading.value = true
      
      // Use /all endpoint for simple list, or regular endpoint for pagination
      const endpoint = params.page ? '/categories' : '/categories/all'
      const response = await apiClient.get(endpoint, { params })

      console.log(response)
      
      if (response.data.success) {
        if (Array.isArray(response.data.data)) {
          // Simple array response from /all endpoint
          categories.value = response.data.data
        } else {
          // Paginated response
          categories.value = response.data.data.items || []
          pagination.value = {
            page: response.data.data.currentPage || 1,
            limit: response.data.data.pageSize || 10,
            total: response.data.data.totalCount || 0,
            totalPages: response.data.data.totalPages || 0
          }
        }
      }
      
      return response.data
    } catch (error) {
      console.error('Error fetching categories:', error)
      throw error
    } finally {
      isLoading.value = false
    }
  }

  const getCategoryById = async (id) => {
    try {
      const response = await apiClient.get(`/categories/${id}`)
      return response.data
    } catch (error) {
      console.error('Error fetching category:', error)
      throw error
    }
  }

  const createCategory = async (categoryData) => {
    try {
      const response = await apiClient.post('/categories', categoryData)
      
      if (response.data.success) {
        categories.value.push(response.data.data)
      }
      
      return response.data
    } catch (error) {
      console.error('Error creating category:', error)
      throw error
    }
  }

  const updateCategory = async (id, categoryData) => {
    try {
      // Ensure the ID is included in the request body
      const requestData = { ...categoryData, id }
      const response = await apiClient.put(`/categories/${id}`, requestData)
      
      if (response.data.success) {
        const index = categories.value.findIndex(c => c.id === id)
        if (index !== -1) {
          categories.value[index] = response.data.data
        }
      }
      
      return response.data
    } catch (error) {
      console.error('Error updating category:', error)
      throw error
    }
  }

  const deleteCategory = async (id) => {
    try {
      const response = await apiClient.delete(`/categories/${id}`)
      
      if (response.data.success) {
        categories.value = categories.value.filter(c => c.id !== id)
      }
      
      return response.data
    } catch (error) {
      console.error('Error deleting category:', error)
      throw error
    }
  }

  return {
    categories,
    isLoading,
    pagination,
    getCategories,
    getCategoryById,
    createCategory,
    updateCategory,
    deleteCategory
  }
})
