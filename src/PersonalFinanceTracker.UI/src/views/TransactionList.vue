<template>
  <div>
    <div v-if="transactions.length === 0" class="text-center py-4">
      <p class="text-muted">No transactions found</p>
    </div>
    
    <div v-else>
      <div
        v-for="transaction in displayTransactions"
        :key="transaction.id"
        class="transaction-item d-flex justify-content-between align-items-center p-3 mb-2 bg-light rounded"
      >
        <div class="d-flex align-items-center">
          <div class="me-3">
            <i
              class="bi fs-4"
              :class="transaction.type === 'income' ? 'bi-arrow-up-circle text-success' : 'bi-arrow-down-circle text-danger'"
            ></i>
          </div>
          <div>
            <h6 class="mb-1">{{ transaction.description }}</h6>
            <small class="text-muted">
              {{ transaction.category?.name }} • {{ formatDate(transaction.date) }}
            </small>
          </div>
        </div>
        
        <div class="text-end">
          <span
            class="fw-bold"
            :class="transaction.type === 'income' ? 'text-success' : 'text-danger'"
          >
            {{ transaction.type === 'income' ? '+' : '-' }}${{ formatCurrency(transaction.amount) }}
          </span>
          
          <div class="mt-1" v-if="!hideActions">
            <button
              class="btn btn-sm btn-outline-primary me-1"
              @click="$emit('edit', transaction)"
            >
              <i class="bi bi-pencil"></i>
            </button>
            <button
              class="btn btn-sm btn-outline-danger"
              @click="$emit('delete', transaction)"
            >
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { computed } from 'vue'

export default {
  name: 'TransactionList',
  props: {
    transactions: {
      type: Array,
      default: () => []
    },
    limit: {
      type: Number,
      default: null
    },
    hideActions: {
      type: Boolean,
      default: false
    }
  },
  emits: ['edit', 'delete'],
  setup(props) {
    const displayTransactions = computed(() => {
      if (props.limit) {
        return props.transactions.slice(0, props.limit)
      }
      return props.transactions
    })
    
    const formatCurrency = (amount) => {
      return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      }).format(amount)
    }
    
    const formatDate = (date) => {
      return new Date(date).toLocaleDateString()
    }
    
    return {
      displayTransactions,
      formatCurrency,
      formatDate
    }
  }
}
</script>
