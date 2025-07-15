
<template>
  <div>
    <div class="row mb-4">
      <div class="col">
        <h1>Reports</h1>
        <p class="text-muted">Financial insights and analytics</p>
      </div>
    </div>
    
    <!-- Date Range Filter -->
    <div class="card mb-4">
      <div class="card-body">
        <div class="row">
          <div class="col-md-3">
            <label class="form-label">From Date</label>
            <input type="date" class="form-control" v-model="dateRange.from" />
          </div>
          <div class="col-md-3">
            <label class="form-label">To Date</label>
            <input type="date" class="form-control" v-model="dateRange.to" />
          </div>
          <div class="col-md-3">
            <label class="form-label">&nbsp;</label>
            <div class="d-grid">
              <button class="btn btn-primary" @click="loadReports" :disabled="isLoading">
                <span v-if="isLoading" class="loading-spinner me-2"></span>
                <i v-else class="bi bi-search"></i> 
                {{ isLoading ? 'Loading...' : 'Generate Reports' }}
              </button>
            </div>
          </div>
          <div class="col-md-3">
            <label class="form-label">&nbsp;</label>
            <div class="d-grid">
              <button 
                class="btn btn-outline-success" 
                @click="exportReports" 
                :disabled="!reportData || isExporting"
              >
                <span v-if="isExporting" class="loading-spinner me-2"></span>
                <i v-else class="bi bi-download"></i> 
                {{ isExporting ? 'Exporting...' : 'Export PDF' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading && !reportData" class="text-center py-5">
      <div class="loading-spinner mb-3" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted">Generating your financial report...</p>
    </div>

    <!-- No Data State -->
    <div v-else-if="!reportData && !isLoading" class="text-center py-5">
      <i class="bi bi-bar-chart display-1 text-muted mb-3"></i>
      <h4 class="text-muted">No Data Available</h4>
      <p class="text-muted">Select a date range and click "Generate Reports" to view your financial insights.</p>
    </div>
    
    <!-- Reports Content -->
    <div v-else-if="reportData">
      <!-- Summary Cards -->
      <div class="row mb-4">
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title text-success">Total Income</h5>
              <h3 class="income">${{ formatCurrency(reportData.totalIncome) }}</h3>
              <small class="text-muted">
                <i class="bi" :class="getIncomeChangeIcon()"></i>
                {{ getIncomeChangeText() }}
              </small>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title text-danger">Total Expenses</h5>
              <h3 class="expense">${{ formatCurrency(reportData.totalExpenses) }}</h3>
              <small class="text-muted">
                <i class="bi" :class="getExpenseChangeIcon()"></i>
                {{ getExpenseChangeText() }}
              </small>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Net Balance</h5>
              <h3 :class="reportData.netBalance >= 0 ? 'balance-positive' : 'balance-negative'">
                ${{ formatCurrency(reportData.netBalance) }}
              </h3>
              <small class="text-muted">
                {{ reportData.netBalance >= 0 ? 'Surplus' : 'Deficit' }}
              </small>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Savings Rate</h5>
              <h3 :class="getSavingsRateClass()">
                {{ reportData.savingsRate }}%
              </h3>
              <small class="text-muted">
                {{ getSavingsRateDescription() }}
              </small>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Charts Section -->
      <div class="row mb-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="mb-0">Income vs Expenses Trend</h5>
              <div class="btn-group btn-group-sm" role="group">
                <input type="radio" class="btn-check" name="trendView" id="monthly" value="monthly" v-model="trendView">
                <label class="btn btn-outline-primary" for="monthly">Monthly</label>
                <input type="radio" class="btn-check" name="trendView" id="weekly" value="weekly" v-model="trendView">
                <label class="btn btn-outline-primary" for="weekly">Weekly</label>
              </div>
            </div>
            <div class="card-body">
              <LineChart v-if="reportData.monthlyTrends" :data="getFilteredTrendData()" />
              <div v-else class="text-center py-4">
                <p class="text-muted">No trend data available</p>
              </div>
            </div>
          </div>
        </div>
        
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Expense Breakdown</h5>
            </div>
            <div class="card-body">
              <PieChart v-if="reportData.expenseBreakdown" :data="reportData.expenseBreakdown" />
              <div v-else class="text-center py-4">
                <p class="text-muted">No expense data available</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Additional Charts Row -->
      <div class="row mb-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Daily Spending Pattern</h5>
            </div>
            <div class="card-body">
              <BarChart v-if="reportData.dailySpending" :data="reportData.dailySpending" />
              <div v-else class="text-center py-4">
                <p class="text-muted">No daily spending data available</p>
              </div>
            </div>
          </div>
        </div>
        
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Budget vs Actual</h5>
            </div>
            <div class="card-body">
              <div v-if="reportData.budgetComparison" class="budget-comparison">
                <div
                  v-for="budget in reportData.budgetComparison"
                  :key="budget.category"
                  class="mb-3"
                >
                  <div class="d-flex justify-content-between align-items-center mb-1">
                    <span>{{ budget.category }}</span>
                    <span class="text-muted">
                      ${{ formatCurrency(budget.actual) }} / ${{ formatCurrency(budget.budget) }}
                    </span>
                  </div>
                  <div class="progress">
                    <div 
                      class="progress-bar" 
                      :class="getBudgetProgressClass(budget)"
                      :style="{ width: getBudgetProgressWidth(budget) + '%' }"
                    ></div>
                  </div>
                  <small :class="getBudgetStatusClass(budget)">
                    {{ getBudgetStatus(budget) }}
                  </small>
                </div>
              </div>
              <div v-else class="text-center py-4">
                <p class="text-muted">No budget data available</p>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Category Analysis -->
      <div class="row mb-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="mb-0">Top Expense Categories</h5>
              <span class="badge bg-danger">{{ reportData.topExpenseCategories?.length || 0 }}</span>
            </div>
            <div class="card-body">
              <div v-if="reportData.topExpenseCategories && reportData.topExpenseCategories.length > 0">
                <div
                  v-for="(category, index) in reportData.topExpenseCategories"
                  :key="category.name"
                  class="d-flex justify-content-between align-items-center mb-3 p-2 rounded"
                  :class="index < 3 ? 'bg-light' : ''"
                >
                  <div class="d-flex align-items-center">
                    <span class="badge me-2" 
                      :class="index === 0 ? 'bg-danger' : index === 1 ? 'bg-warning' : index === 2 ? 'bg-info' : 'bg-secondary'">
                      {{ index + 1 }}
                    </span>
                    <span>{{ category.name }}</span>
                  </div>
                  <div class="text-end">
                    <div class="fw-bold text-danger">${{ formatCurrency(category.amount) }}</div>
                    <small class="text-muted">{{ getPercentage(category.amount, reportData.totalExpenses) }}%</small>
                  </div>
                </div>
              </div>
              <div v-else class="text-center py-4">
                <p class="text-muted">No expense categories found</p>
              </div>
            </div>
          </div>
        </div>
        
        <div class="col-md-6">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="mb-0">Income Sources</h5>
              <span class="badge bg-success">{{ reportData.incomeSources?.length || 0 }}</span>
            </div>
            <div class="card-body">
              <div v-if="reportData.incomeSources && reportData.incomeSources.length > 0">
                <div
                  v-for="(source, index) in reportData.incomeSources"
                  :key="source.name"
                  class="d-flex justify-content-between align-items-center mb-3 p-2 rounded"
                  :class="index < 3 ? 'bg-light' : ''"
                >
                  <div class="d-flex align-items-center">
                    <span class="badge me-2" 
                      :class="index === 0 ? 'bg-success' : index === 1 ? 'bg-info' : index === 2 ? 'bg-warning' : 'bg-secondary'">
                      {{ index + 1 }}
                    </span>
                    <span>{{ source.name }}</span>
                  </div>
                  <div class="text-end">
                    <div class="fw-bold text-success">${{ formatCurrency(source.amount) }}</div>
                    <small class="text-muted">{{ getPercentage(source.amount, reportData.totalIncome) }}%</small>
                  </div>
                </div>
              </div>
              <div v-else class="text-center py-4">
                <p class="text-muted">No income sources found</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Financial Insights -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Financial Insights & Recommendations</h5>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-4">
                  <div class="insight-card p-3 mb-3 border rounded">
                    <div class="d-flex align-items-center mb-2">
                      <i class="bi bi-lightbulb text-warning me-2"></i>
                      <h6 class="mb-0">Spending Insight</h6>
                    </div>
                    <p class="text-muted mb-0">{{ getSpendingInsight() }}</p>
                  </div>
                </div>
                
                <div class="col-md-4">
                  <div class="insight-card p-3 mb-3 border rounded">
                    <div class="d-flex align-items-center mb-2">
                      <i class="bi bi-target text-info me-2"></i>
                      <h6 class="mb-0">Savings Goal</h6>
                    </div>
                    <p class="text-muted mb-0">{{ getSavingsRecommendation() }}</p>
                  </div>
                </div>
                
                <div class="col-md-4">
                  <div class="insight-card p-3 mb-3 border rounded">
                    <div class="d-flex align-items-center mb-2">
                      <i class="bi bi-graph-up text-success me-2"></i>
                      <h6 class="mb-0">Growth Opportunity</h6>
                    </div>
                    <p class="text-muted mb-0">{{ getGrowthOpportunity() }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive, onMounted } from 'vue'
import { useToastStore } from '@/stores/toast'
import LineChart from '@/components/charts/LineChart.vue'
import PieChart from '@/components/charts/PieChart.vue'
import BarChart from '@/components/charts/BarChart.vue'

export default {
  name: 'ReportsView',
  components: {
    LineChart,
    PieChart,
    BarChart
  },
  setup() {
    const toastStore = useToastStore()
    
    const dateRange = reactive({
      from: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0],
      to: new Date().toISOString().split('T')[0]
    })
    
    const reportData = ref(null)
    const isLoading = ref(false)
    const isExporting = ref(false)
    const trendView = ref('monthly')
    
    const formatCurrency = (amount) => {
      return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      }).format(amount)
    }

    const getPercentage = (amount, total) => {
      if (!total || total === 0) return 0
      return ((amount / total) * 100).toFixed(1)
    }

    const getSavingsRateClass = () => {
      if (!reportData.value) return 'text-muted'
      const rate = reportData.value.savingsRate
      if (rate >= 20) return 'text-success'
      if (rate >= 10) return 'text-warning'
      return 'text-danger'
    }

    const getSavingsRateDescription = () => {
      if (!reportData.value) return ''
      const rate = reportData.value.savingsRate
      if (rate >= 20) return 'Excellent'
      if (rate >= 10) return 'Good'
      if (rate >= 5) return 'Fair'
      return 'Needs Improvement'
    }

    const getIncomeChangeIcon = () => {
      if (!reportData.value?.incomeChange) return 'bi-dash'
      return reportData.value.incomeChange >= 0 ? 'bi-arrow-up text-success' : 'bi-arrow-down text-danger'
    }

    const getIncomeChangeText = () => {
      if (!reportData.value?.incomeChange) return 'No change data'
      const change = Math.abs(reportData.value.incomeChange)
      const direction = reportData.value.incomeChange >= 0 ? 'increase' : 'decrease'
      return `${change.toFixed(1)}% ${direction} vs last period`
    }

    const getExpenseChangeIcon = () => {
      if (!reportData.value?.expenseChange) return 'bi-dash'
      return reportData.value.expenseChange >= 0 ? 'bi-arrow-up text-danger' : 'bi-arrow-down text-success'
    }

    const getExpenseChangeText = () => {
      if (!reportData.value?.expenseChange) return 'No change data'
      const change = Math.abs(reportData.value.expenseChange)
      const direction = reportData.value.expenseChange >= 0 ? 'increase' : 'decrease'
      return `${change.toFixed(1)}% ${direction} vs last period`
    }

    const getBudgetProgressClass = (budget) => {
      const percentage = (budget.actual / budget.budget) * 100
      if (percentage <= 75) return 'bg-success'
      if (percentage <= 90) return 'bg-warning'
      return 'bg-danger'
    }

    const getBudgetProgressWidth = (budget) => {
      return Math.min((budget.actual / budget.budget) * 100, 100)
    }

    const getBudgetStatusClass = (budget) => {
      const percentage = (budget.actual / budget.budget) * 100
      if (percentage <= 75) return 'text-success'
      if (percentage <= 90) return 'text-warning'
      return 'text-danger'
    }

    const getBudgetStatus = (budget) => {
      const percentage = (budget.actual / budget.budget) * 100
      const remaining = budget.budget - budget.actual
      
      if (percentage <= 75) {
        return `$${formatCurrency(remaining)} remaining`
      } else if (percentage <= 100) {
        return `${(100 - percentage).toFixed(1)}% remaining`
      } else {
        return `$${formatCurrency(Math.abs(remaining))} over budget`
      }
    }

    const getFilteredTrendData = () => {
      if (!reportData.value?.monthlyTrends) return null
      
      if (trendView.value === 'weekly') {
        return reportData.value.weeklyTrends || reportData.value.monthlyTrends
      }
      return reportData.value.monthlyTrends
    }

    const getSpendingInsight = () => {
      if (!reportData.value) return 'Generate a report to see insights'
      
      const { totalExpenses, topExpenseCategories } = reportData.value
      
      if (!topExpenseCategories || topExpenseCategories.length === 0) {
        return 'No spending data available for analysis'
      }
      
      const topCategory = topExpenseCategories[0]
      const percentage = getPercentage(topCategory.amount, totalExpenses)
      
      if (percentage > 40) {
        return `${topCategory.name} accounts for ${percentage}% of your spending. Consider reviewing this category for savings opportunities.`
      } else if (percentage > 25) {
        return `Your spending is fairly distributed, with ${topCategory.name} being the largest at ${percentage}%.`
      } else {
        return `Your spending is well-diversified across categories. Great job maintaining balance!`
      }
    }

    const getSavingsRecommendation = () => {
      if (!reportData.value) return 'Generate a report to see recommendations'
      
      const { savingsRate, netBalance } = reportData.value
      
      if (savingsRate >= 20) {
        return 'Excellent savings rate! Consider investing the surplus for long-term growth.'
      } else if (savingsRate >= 10) {
        return 'Good savings rate. Try to increase it to 20% by reducing discretionary spending.'
      } else if (netBalance > 0) {
        return 'You have a positive balance but low savings rate. Focus on consistent saving habits.'
      } else {
        return 'Focus on reducing expenses to achieve a positive savings rate of at least 10%.'
      }
    }

    const getGrowthOpportunity = () => {
      if (!reportData.value) return 'Generate a report to see opportunities'
      
      const { incomeSources, totalIncome } = reportData.value
      
      if (!incomeSources || incomeSources.length === 0) {
        return 'Diversify your income sources to reduce financial risk.'
      } else if (incomeSources.length === 1) {
        return 'Consider developing additional income streams to increase financial security.'
      } else {
        const primarySource = incomeSources[0]
        const primaryPercentage = getPercentage(primarySource.amount, totalIncome)
        
        if (primaryPercentage > 80) {
          return 'Your income is heavily dependent on one source. Consider diversifying.'
        } else {
          return 'Good income diversification! Focus on growing your highest-potential streams.'
        }
      }
    }
    
    const loadReports = async () => {
      try {
        isLoading.value = true
        
        // Validate date range
        if (new Date(dateRange.from) > new Date(dateRange.to)) {
          toastStore.error('From date cannot be after To date')
          return
        }
        
        // Simulate loading with mock data for now
        await new Promise(resolve => setTimeout(resolve, 2000))
        
        // Mock report data
        reportData.value = {
          totalIncome: 5500,
          totalExpenses: 3800,
          netBalance: 1700,
          savingsRate: 31,
          incomeChange: 5.2,
          expenseChange: -2.1,
          monthlyTrends: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
            income: [5000, 5200, 5300, 5400, 5500, 5600],
            expenses: [3500, 3600, 3700, 3750, 3800, 3850]
          },
          expenseBreakdown: {
            labels: ['Food & Dining', 'Transportation', 'Shopping', 'Entertainment', 'Bills'],
            values: [1200, 600, 800, 400, 800]
          },
          dailySpending: {
            labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
            values: [120, 85, 95, 110, 140, 200, 180]
          },
          budgetComparison: [
            { category: 'Food', budget: 1500, actual: 1200 },
            { category: 'Transport', budget: 500, actual: 600 },
            { category: 'Entertainment', budget: 300, actual: 400 }
          ],
          topExpenseCategories: [
            { name: 'Food & Dining', amount: 1200 },
            { name: 'Shopping', amount: 800 },
            { name: 'Bills & Utilities', amount: 800 },
            { name: 'Transportation', amount: 600 },
            { name: 'Entertainment', amount: 400 }
          ],
          incomeSources: [
            { name: 'Salary', amount: 4500 },
            { name: 'Freelance', amount: 800 },
            { name: 'Investments', amount: 200 }
          ]
        }
        
        toastStore.success(`Report generated: $${formatCurrency(reportData.value.totalIncome)} income, $${formatCurrency(reportData.value.totalExpenses)} expenses`)
      } catch (error) {
        console.error('Error loading reports:', error)
        toastStore.error('Failed to load reports. Please try again.')
      } finally {
        isLoading.value = false
      }
    }
    
    const exportReports = async () => {
      try {
        isExporting.value = true
        
        // Simulate export
        await new Promise(resolve => setTimeout(resolve, 2000))
        
        toastStore.success('Report exported successfully!')
      } catch (error) {
        console.error('Error exporting reports:', error)
        toastStore.error('Failed to export reports. Please try again.')
      } finally {
        isExporting.value = false
      }
    }

    // Auto-load reports on component mount
    onMounted(() => {
      loadReports()
    })
    
    return {
      dateRange,
      reportData,
      isLoading,
      isExporting,
      trendView,
      formatCurrency,
      getPercentage,
      getSavingsRateClass,
      getSavingsRateDescription,
      getIncomeChangeIcon,
      getIncomeChangeText,
      getExpenseChangeIcon,
      getExpenseChangeText,
      getBudgetProgressClass,
      getBudgetProgressWidth,
      getBudgetStatusClass,
      getBudgetStatus,
      getFilteredTrendData,
      getSpendingInsight,
      getSavingsRecommendation,
      getGrowthOpportunity,
      loadReports,
      exportReports
    }
  }
}
</script>

<style scoped>
.insight-card {
  transition: all 0.3s ease;
}

.insight-card:hover {
  box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
}

.budget-comparison .progress {
  height: 8px;
}

.loading-spinner {
  display: inline-block;
  width: 1rem;
  height: 1rem;
  border: 2px solid #f3f3f3;
  border-top: 2px solid var(--primary-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.card-header .btn-group {
  margin-left: auto;
}

.badge {
  font-size: 0.75em;
}
</style>