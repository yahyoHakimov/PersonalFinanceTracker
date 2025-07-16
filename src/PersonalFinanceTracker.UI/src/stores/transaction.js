import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/api'

export const useTransactionStore = defineStore('transaction', () => {
  const transactions = ref([])
  const categories = ref([])
  const isLoading = ref(false)
  const pagination = ref({
    page: 1,
    limit: 10,
    total: 0,
    totalPages: 0
  })

  const getTransactions = async (params = {}) => {
    try {
      isLoading.value = true
      const response = await apiClient.get('/transactions', { params })
      
      if (response.data.success) {
        transactions.value = response.data.data.items || response.data.data
        
        // Handle pagination if it exists
        if (response.data.data.totalCount !== undefined) {
          pagination.value = {
            page: response.data.data.currentPage || params.page || 1,
            limit: response.data.data.pageSize || params.limit || 10,
            total: response.data.data.totalCount,
            totalPages: response.data.data.totalPages || Math.ceil(response.data.data.totalCount / (params.limit || 10))
          }
        }
      }
      
      return response.data
    } catch (error) {
      console.error('Error fetching transactions:', error)
      throw error
    } finally {
      isLoading.value = false
    }
  }

  const getTransactionById = async (id) => {
    try {
      const response = await apiClient.get(`/transactions/${id}`)
      return response.data
    } catch (error) {
      console.error('Error fetching transaction:', error)
      throw error
    }
  }

  const createTransaction = async (transactionData) => {
  try {
    const response = await apiClient.post('/transactions', transactionData)
    
    // Check if response is successful
    if (response.status === 200 || response.status === 201) {
      if (response.data.success) {
        transactions.value.unshift(response.data.data)
      }
      
      return {
        success: true,
        data: response.data.data
      }
    } else {
      return {
        success: false,
        message: 'Failed to create transaction'
      }
    }
  } catch (error) {
    console.error('Error creating transaction:', error)
    
    // Check if it's a timeout error
    if (error.code === 'ECONNABORTED' && error.message.includes('timeout')) {
      return {
        success: false,
        message: 'Request timeout - please check if transaction was created',
        isTimeout: true
      }
    }
    
    // Return error response instead of throwing
    return {
      success: false,
      message: error.response?.data?.message || error.message || 'Failed to create transaction'
    }
  }
}

  const updateTransaction = async (id, transactionData) => {
    try {
      // Ensure the ID is included in the request body for your API
      const requestData = { ...transactionData, id }
      const response = await apiClient.put(`/transactions/${id}`, requestData)
      
      if (response.data.success) {
        const index = transactions.value.findIndex(t => t.id === id)
        if (index !== -1) {
          transactions.value[index] = response.data.data
        }
      }
      
      return response.data
    } catch (error) {
      console.error('Error updating transaction:', error)
      throw error
    }
  }

  const deleteTransaction = async (id) => {
    try {
      const response = await apiClient.delete(`/transactions/${id}`)
      
      if (response.data.success) {
        transactions.value = transactions.value.filter(t => t.id !== id)
      }
      
      return response.data
    } catch (error) {
      console.error('Error deleting transaction:', error)
      throw error
    }
  }

  const getMonthlyBalance = async (year = null, month = null) => {
    try {
      let url = '/summary/current-month'
      
      // If specific year/month provided, use the transactions endpoint
      if (year && month) {
        url = `/transactions/monthly-balance/${year}/${month}`
      }
      
      const response = await apiClient.get(url)
      
      if (response.data.success) {
        return response.data.data
      }
      
      return null
    } catch (error) {
      console.error('Error fetching monthly balance:', error)
      throw error
    }
  }

  const getMonthlyTrends = async () => {
    try {
      const response = await apiClient.get('/summary/trend')
      
      if (response.data.success) {
        const monthlyData = response.data.data
        
        // Transform the data for chart consumption
        return {
          labels: monthlyData.map(item => {
            const date = new Date(item.year, item.month - 1)
            return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' })
          }),
          income: monthlyData.map(item => item.totalIncome),
          expenses: monthlyData.map(item => item.totalExpense)
        }
      }
      
      return null
    } catch (error) {
      console.error('Error fetching monthly trends:', error)
      throw error
    }
  }

  const getExpenseCategories = async () => {
    try {
      // This might need to be implemented as a separate endpoint
      // For now, we'll calculate from recent transactions
      const response = await apiClient.get('/transactions/all')
      
      if (response.data.success) {
        const allTransactions = response.data.data
        const expenses = allTransactions.filter(t => t.type === 'expense')
        
        // Group by category
        const categoryTotals = {}
        expenses.forEach(expense => {
          const categoryName = expense.category?.name || 'Other'
          categoryTotals[categoryName] = (categoryTotals[categoryName] || 0) + expense.amount
        })
        
        return {
          labels: Object.keys(categoryTotals),
          values: Object.values(categoryTotals)
        }
      }
      
      return null
    } catch (error) {
      console.error('Error fetching expense categories:', error)
      throw error
    }
  }

  const getRecentTransactions = async (limit = 5) => {
    try {
      const response = await apiClient.get('/transactions', {
        params: { page: 1, limit }
      })
      
      if (response.data.success) {
        return response.data.data.items || response.data.data
      }
      
      return []
    } catch (error) {
      console.error('Error fetching recent transactions:', error)
      throw error
    }
  }

  const getCategories = async () => {
    try {
      const response = await apiClient.get('/categories/all')
      
      if (response.data.success) {
        categories.value = response.data.data
      }
      
      return response.data
    } catch (error) {
      console.error('Error fetching categories:', error)
      throw error
    }
  }

  return {
    transactions,
    categories,
    isLoading,
    pagination,
    getTransactions,
    getTransactionById,
    createTransaction,
    updateTransaction,
    deleteTransaction,
    getMonthlyBalance,
    getMonthlyTrends,
    getExpenseCategories,
    getRecentTransactions,
    getCategories
  }
})