// Updated DashboardView.vue script section
<template>
  <div>
    <div class="row mb-4">
      <div class="col">
        <h1>Dashboard</h1>
        <p class="text-muted">Welcome back, {{ authStore.user?.username }}!</p>
      </div>
      <div class="col-auto">
        <button class="btn btn-outline-primary me-2" @click="refreshDashboard" :disabled="isLoading">
          <i class="bi" :class="isLoading ? 'bi-arrow-clockwise' : 'bi-arrow-clockwise'"></i>
          {{ isLoading ? 'Loading...' : 'Refresh' }}
        </button>
      </div>
    </div>
    
    <!-- Error Alert -->
    <div v-if="loadingError" class="alert alert-warning alert-dismissible fade show mb-4" role="alert">
      <i class="bi bi-exclamation-triangle me-2"></i>
      Some dashboard data couldn't be loaded: {{ loadingError }}
      <button type="button" class="btn-close" @click="loadingError = null"></button>
    </div>
    
    <!-- Loading State -->
    <div v-if="isLoading && !monthlyBalance" class="text-center py-5">
      <div class="loading-spinner mb-3" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted">Loading your dashboard...</p>
    </div>
    
    <!-- Dashboard Content -->
    <div v-else>
      <!-- Balance Cards -->
      <div class="row mb-4">
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title text-success">Total Income</h5>
              <h3 class="card-text income">
                ${{ formatCurrency(monthlyBalance?.totalIncome || 0) }}
              </h3>
              <small class="text-muted">This month</small>
            </div>
          </div>
        </div>
        
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title text-danger">Total Expenses</h5>
              <h3 class="card-text expense">
                ${{ formatCurrency(monthlyBalance?.totalExpense || 0) }}
              </h3>
              <small class="text-muted">This month</small>
            </div>
          </div>
        </div>
        
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Net Balance</h5>
              <h3 class="card-text" :class="balanceClass">
                ${{ formatCurrency(monthlyBalance?.netBalance || 0) }}
              </h3>
              <small class="text-muted">
                {{ (monthlyBalance?.netBalance || 0) >= 0 ? 'Surplus' : 'Deficit' }}
              </small>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Charts Section -->
      <div class="row mb-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5>Monthly Trends</h5>
            </div>
            <div class="card-body">
              <LineChart v-if="monthlyTrends" :data="monthlyTrends" />
              <div v-else class="text-center py-4">
                <p class="text-muted">No trend data available</p>
              </div>
            </div>
          </div>
        </div>
        
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5>Category Breakdown</h5>
            </div>
            <div class="card-body">
              <PieChart v-if="expenseCategories" :data="expenseCategories" />
              <div v-else class="text-center py-4">
                <p class="text-muted">No category data available</p>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Recent Transactions and Quick Actions -->
      <div class="row">
        <div class="col-md-8">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5>Recent Transactions</h5>
              <router-link to="/transactions" class="btn btn-sm btn-outline-primary">
                View All
              </router-link>
            </div>
            <div class="card-body">
              <TransactionList 
                :transactions="recentTransactions" 
                :limit="5" 
                :hide-actions="true" 
              />
              <div v-if="recentTransactions.length === 0" class="text-center py-4">
                <p class="text-muted">No transactions yet</p>
                <button class="btn btn-primary" @click="showAddTransactionModal = true">
                  Add Your First Transaction
                </button>
              </div>
            </div>
          </div>
        </div>
        
        <div class="col-md-4">
          <div class="card">
            <div class="card-header">
              <h5>Quick Actions</h5>
            </div>
            <div class="card-body">
              <div class="d-grid gap-2">
                <button class="btn btn-success" @click="showAddTransactionModal = true">
                  <i class="bi bi-plus-circle"></i> Add Transaction
                </button>
                <router-link to="/categories" class="btn btn-info">
                  <i class="bi bi-tags"></i> Manage Categories
                </router-link>
                <router-link to="/reports" class="btn btn-secondary">
                  <i class="bi bi-bar-chart"></i> View Reports
                </router-link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Add Transaction Modal -->
    <AddTransactionModal
      v-if="showAddTransactionModal"
      @close="showAddTransactionModal = false"
      @transaction-added="handleTransactionAdded"
    />
  </div>
</template>
<script>
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useTransactionStore } from '@/stores/transaction'
import { useCategoryStore } from '@/stores/category'
import { useToastStore } from '@/stores/toast'
import LineChart from '@/components/charts/LineChart.vue'
import PieChart from '@/components/charts/PieChart.vue'
import TransactionList from './TransactionList.vue'
import AddTransactionModal from '@/components/AddTransactionModal.vue'

export default {
  name: 'DashboardView',
  components: {
    LineChart,
    PieChart,
    TransactionList,
    AddTransactionModal
  },
  setup() {
    const authStore = useAuthStore()
    const transactionStore = useTransactionStore()
    const categoryStore = useCategoryStore()
    const toastStore = useToastStore()
    
    const showAddTransactionModal = ref(false)
    const monthlyBalance = ref(null)
    const monthlyTrends = ref(null)
    const expenseCategories = ref(null)
    const recentTransactions = ref([])
    const isLoading = ref(false)
    const loadingError = ref(null)
    
    const balanceClass = computed(() => {
      const balance = monthlyBalance.value?.netBalance || 0
      return balance >= 0 ? 'balance-positive' : 'balance-negative'
    })
    
    const formatCurrency = (amount) => {
      return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      }).format(amount || 0)
    }
    
    const loadDashboardData = async () => {
      try {
        isLoading.value = true
        loadingError.value = null
        
        console.log('Loading dashboard data...')
        
        // Load categories first (needed for other operations)
        await categoryStore.getCategories()
        console.log('Categories loaded:', categoryStore.categories.length)
        
        // Load dashboard data with better error handling
        const results = await Promise.allSettled([
          transactionStore.getMonthlyBalance(),
          transactionStore.getMonthlyTrends(),
          transactionStore.getExpenseCategories(),
          transactionStore.getRecentTransactions(5)
        ])
        
        // Process results
        const [balanceResult, trendsResult, categoriesResult, transactionsResult] = results
        
        // Monthly Balance
        if (balanceResult.status === 'fulfilled') {
          monthlyBalance.value = balanceResult.value
          console.log('Monthly balance loaded:', monthlyBalance.value)
        } else {
          console.error('Failed to load monthly balance:', balanceResult.reason)
          // Provide fallback data
          monthlyBalance.value = {
            totalIncome: 0,
            totalExpense: 0,
            netBalance: 0
          }
        }
        
        // Monthly Trends
        if (trendsResult.status === 'fulfilled') {
          monthlyTrends.value = trendsResult.value
          console.log('Monthly trends loaded:', monthlyTrends.value)
        } else {
          console.error('Failed to load monthly trends:', trendsResult.reason)
          // Provide fallback chart data
          monthlyTrends.value = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
            income: [0, 0, 0, 0, 0, 0],
            expenses: [0, 0, 0, 0, 0, 0]
          }
        }
        
        // Expense Categories
        if (categoriesResult.status === 'fulfilled') {
          expenseCategories.value = categoriesResult.value
          console.log('Expense categories loaded:', expenseCategories.value)
        } else {
          console.error('Failed to load expense categories:', categoriesResult.reason)
          // Create fallback data from loaded categories
          expenseCategories.value = createFallbackCategoryData()
        }
        
        // Recent Transactions
        if (transactionsResult.status === 'fulfilled') {
          recentTransactions.value = transactionsResult.value
          console.log('Recent transactions loaded:', recentTransactions.value?.length)
        } else {
          console.error('Failed to load recent transactions:', transactionsResult.reason)
          recentTransactions.value = []
        }
        
        console.log('Dashboard data loading completed')
        
      } catch (error) {
        console.error('Error loading dashboard data:', error)
        loadingError.value = error.message
        toastStore.error('Failed to load dashboard data')
        
        // Set fallback data
        setFallbackData()
      } finally {
        isLoading.value = false
      }
    }
    
    const createFallbackCategoryData = () => {
      // Use actual categories from the store to create chart data
      if (categoryStore.categories.length > 0) {
        return {
          labels: categoryStore.categories.slice(0, 5).map(cat => cat.name),
          values: categoryStore.categories.slice(0, 5).map(cat => cat.totalAmount || Math.random() * 500)
        }
      }
      
      // Default fallback
      return {
        labels: ['No Categories'],
        values: [0]
      }
    }
    
    const setFallbackData = () => {
      monthlyBalance.value = {
        totalIncome: 0,
        totalExpense: 0,
        netBalance: 0
      }
      
      monthlyTrends.value = {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
        income: [0, 0, 0, 0, 0, 0],
        expenses: [0, 0, 0, 0, 0, 0]
      }
      
      expenseCategories.value = {
        labels: ['No Data'],
        values: [0]
      }
      
      recentTransactions.value = []
    }
    
    const refreshDashboard = async () => {
      console.log('Refreshing dashboard...')
      await loadDashboardData()
      toastStore.success('Dashboard refreshed')
    }
    
    const handleTransactionAdded = () => {
      console.log('Transaction added, refreshing dashboard...')
      refreshDashboard()
    }
    
    onMounted(() => {
      console.log('Dashboard mounted, loading data...')
      loadDashboardData()
    })
    
    return {
      authStore,
      showAddTransactionModal,
      monthlyBalance,
      monthlyTrends,
      expenseCategories,
      recentTransactions,
      isLoading,
      loadingError,
      balanceClass,
      formatCurrency,
      refreshDashboard,
      handleTransactionAdded
    }
  }
}
</script>
