<template>
  <div>
    <div class="row mb-4">
      <div class="col-md-6">
        <h1>Transactions</h1>
      </div>
      <div class="col-md-6 text-end">
        <button class="btn btn-primary" @click="showAddModal = true">
          <i class="bi bi-plus"></i> Add Transaction
        </button>
      </div>
    </div>
    
    <!-- Filters -->
    <div class="card mb-4">
      <div class="card-body">
        <div class="row">
          <div class="col-md-3">
            <label class="form-label">Type</label>
            <select class="form-select" v-model="filters.type">
              <option value="">All Types</option>
              <option value="income">Income</option>
              <option value="expense">Expense</option>
            </select>
          </div>
          
          <div class="col-md-3">
            <label class="form-label">Category</label>
            <select class="form-select" v-model="filters.categoryId">
              <option value="">All Categories</option>
              <option
                v-for="category in transactionStore.categories"
                :key="category.id"
                :value="category.id"
              >
                {{ category.name }}
              </option>
            </select>
          </div>
          
          <div class="col-md-2">
            <label class="form-label">From Date</label>
            <input type="date" class="form-control" v-model="filters.fromDate" />
          </div>
          
          <div class="col-md-2">
            <label class="form-label">To Date</label>
            <input type="date" class="form-control" v-model="filters.toDate" />
          </div>
          
          <div class="col-md-2">
            <label class="form-label">&nbsp;</label>
            <div class="d-grid">
              <button class="btn btn-outline-primary" @click="applyFilters">
                <i class="bi bi-funnel"></i> Filter
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Transactions List -->
    <div class="card">
      <div class="card-body">
        <div v-if="transactionStore.isLoading" class="text-center py-4">
          <div class="loading-spinner"></div>
          <p class="mt-2">Loading transactions...</p>
        </div>
        
        <TransactionList
          v-else
          :transactions="transactionStore.transactions"
          @edit="editTransaction"
          @delete="deleteTransaction"
        />
        
        <!-- Pagination -->
        <div v-if="transactionStore.pagination.totalPages > 1" class="mt-4">
          <nav>
            <ul class="pagination justify-content-center">
              <li class="page-item" :class="{ disabled: transactionStore.pagination.page === 1 }">
                <button class="page-link" @click="changePage(transactionStore.pagination.page - 1)">
                  Previous
                </button>
              </li>
              
              <li
                v-for="page in totalPages"
                :key="page"
                class="page-item"
                :class="{ active: page === transactionStore.pagination.page }"
              >
                <button class="page-link" @click="changePage(page)">
                  {{ page }}
                </button>
              </li>
              
              <li class="page-item" :class="{ disabled: transactionStore.pagination.page === transactionStore.pagination.totalPages }">
                <button class="page-link" @click="changePage(transactionStore.pagination.page + 1)">
                  Next
                </button>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
    
    <!-- Add Transaction Modal -->
    <AddTransactionModal
      v-if="showAddModal"
      @close="showAddModal = false"
      @transaction-added="refreshTransactions"
    />
    
    <!-- Edit Transaction Modal -->
    <EditTransactionModal
      v-if="showEditModal"
      :transaction="selectedTransaction"
      @close="showEditModal = false"
      @transaction-updated="refreshTransactions"
    />
  </div>
</template>

<script>
import { ref, reactive, computed, onMounted } from 'vue'
import { useTransactionStore } from '@/stores/transaction'
import { useToastStore } from '@/stores/toast'
import TransactionList from './TransactionList.vue'
import AddTransactionModal from '@/components/AddTransactionModal.vue'
import EditTransactionModal from '@/components/EditTransactionModal.vue'

export default {
  name: 'TransactionsView',
  components: {
    TransactionList,
    AddTransactionModal,
    EditTransactionModal
  },
  setup() {
    const transactionStore = useTransactionStore()
    const toastStore = useToastStore()
    
    const showAddModal = ref(false)
    const showEditModal = ref(false)
    const selectedTransaction = ref(null)
    
    const filters = reactive({
      type: '',
      categoryId: '',
      fromDate: '',
      toDate: ''
    })
    
    const totalPages = computed(() => {
      const pages = []
      for (let i = 1; i <= transactionStore.pagination.totalPages; i++) {
        pages.push(i)
      }
      return pages
    })
    
    const applyFilters = () => {
      loadTransactions()
    }
    
    const changePage = (page) => {
      if (page >= 1 && page <= transactionStore.pagination.totalPages) {
        loadTransactions(page)
      }
    }
    
    const loadTransactions = async (page = 1) => {
      const params = {
        page,
        limit: 10,
        ...filters
      }
      
      // Remove empty filter values
      Object.keys(params).forEach(key => {
        if (params[key] === '' || params[key] === null || params[key] === undefined) {
          delete params[key]
        }
      })
      
      await transactionStore.getTransactions(params)
    }
    
    const editTransaction = (transaction) => {
      selectedTransaction.value = transaction
      showEditModal.value = true
    }
    
    const deleteTransaction = async (transaction) => {
      if (confirm('Are you sure you want to delete this transaction?')) {
        try {
          await transactionStore.deleteTransaction(transaction.id)
          toastStore.success('Transaction deleted successfully!')
          refreshTransactions()
        } catch (error) {
          toastStore.error('Failed to delete transaction')
        }
      }
    }
    
    const refreshTransactions = () => {
      loadTransactions(transactionStore.pagination.page)
    }
    
    onMounted(() => {
      transactionStore.getCategories()
      loadTransactions()
    })
    
    return {
      transactionStore,
      showAddModal,
      showEditModal,
      selectedTransaction,
      filters,
      totalPages,
      applyFilters,
      changePage,
      editTransaction,
      deleteTransaction,
      refreshTransactions
    }
  }
}
</script>