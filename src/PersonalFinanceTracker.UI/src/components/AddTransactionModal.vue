<template>
  <div class="modal fade show d-block" style="background-color: rgba(0,0,0,0.5)">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">Add Transaction</h5>
          <button type="button" class="btn-close" @click="$emit('close')"></button>
        </div>
        
        <form @submit.prevent="handleSubmit">
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Type</label>
              <div class="btn-group w-100" role="group">
                <input
                  type="radio"
                  class="btn-check"
                  name="type"
                  id="income"
                  value="income"
                  v-model="form.type"
                >
                <label class="btn btn-outline-success" for="income">Income</label>
                
                <input
                  type="radio"
                  class="btn-check"
                  name="type"
                  id="expense"
                  value="expense"
                  v-model="form.type"
                >
                <label class="btn btn-outline-danger" for="expense">Expense</label>
              </div>
            </div>
            
            <div class="mb-3">
              <label for="amount" class="form-label">Amount</label>
              <input
                type="number"
                step="0.01"
                class="form-control"
                id="amount"
                v-model="form.amount"
                :class="{ 'is-invalid': errors.amount }"
                required
              />
              <div v-if="errors.amount" class="invalid-feedback">
                {{ errors.amount }}
              </div>
            </div>
            
            <div class="mb-3">
              <label for="categoryId" class="form-label">Category</label>
              <select
                class="form-select"
                id="categoryId"
                v-model="form.categoryId"
                :class="{ 'is-invalid': errors.categoryId }"
                required
              >
                <option value="">Select a category</option>
                <option
                  v-for="category in categoryStore.categories"
                  :key="category.id"
                  :value="category.id"
                >
                  {{ category.name }} ({{ category.transactionCount || 0 }} transactions)
                </option>
              </select>
              <div v-if="errors.categoryId" class="invalid-feedback">
                {{ errors.categoryId }}
              </div>
              
              <!-- DEBUG INFO -->
              <small class="text-muted">
                Available categories: {{ categoryStore.categories.length }}
              </small>
            </div>
            
            <div class="mb-3">
              <label for="notes" class="form-label">Notes (Optional)</label>
              <textarea
                class="form-control"
                id="notes"
                rows="3"
                v-model="form.notes"
              ></textarea>
            </div>
          </div>
          
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="$emit('close')">
              Cancel
            </button>
            <button type="submit" class="btn btn-primary" :disabled="isLoading">
              <span v-if="isLoading" class="loading-spinner me-2"></span>
              {{ isLoading ? 'Saving...' : 'Save Transaction' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive, onMounted } from 'vue'
import { useTransactionStore } from '@/stores/transaction'
import { useCategoryStore } from '@/stores/category'
import { useToastStore } from '@/stores/toast'

export default {
  name: 'AddTransactionModal',
  emits: ['close', 'transaction-added'],
  setup(props, { emit }) {
    const transactionStore = useTransactionStore()
    const categoryStore = useCategoryStore()
    const toastStore = useToastStore()
    
    const form = reactive({
      type: 'expense',
      amount: '',
      categoryId: '',
      notes: ''
    })
    
    const errors = ref({})
    const isLoading = ref(false)
    
    const handleSubmit = async () => {
      errors.value = {}
      
      // Basic validation - only validate fields that exist in the form
      if (!form.amount || form.amount <= 0) {
        errors.value.amount = 'Amount must be greater than 0'
      }
      
      if (!form.categoryId) {
        errors.value.categoryId = 'Category is required'
      }
      
      if (Object.keys(errors.value).length > 0) {
        return
      }
      
      try {
        isLoading.value = true
        
        // Prepare data to match your C# DTO
        const transactionData = {
          amount: parseFloat(form.amount),
          type: form.type === 'income' ? 0 : 1, // Map to enum: Income = 0, Expense = 1
          categoryId: parseInt(form.categoryId),
          note: form.notes || null // Map 'notes' to 'note' to match C# DTO
        }
        
        console.log('Form data before mapping:', form)
        console.log('Sending transaction data:', transactionData)
        console.log('Type mapping:', form.type, '->', transactionData.type)
        
        const result = await transactionStore.createTransaction(transactionData)
        
        if (result.success) {
          toastStore.success('Transaction added successfully!')
          emit('transaction-added')
          emit('close')
        } else {
          toastStore.error(result.message || 'Failed to add transaction')
        }
      } catch (error) {
        console.error('Transaction creation error:', error)
        
        // Log detailed error information
        if (error.response?.data) {
          console.error('Server response:', error.response.data)
          console.error('Status:', error.response.status)
          
          // Show validation errors if available
          if (error.response.data.errors) {
            console.error('Validation errors:', error.response.data.errors)
            
            // Log each validation error in detail
            Object.keys(error.response.data.errors).forEach(key => {
              console.error(`Field ${key}:`, error.response.data.errors[key])
            })
          }
        }
        
        toastStore.error('Failed to add transaction')
      } finally {
        isLoading.value = false
      }
    }
    
    onMounted(async () => {
      console.log('Loading categories for transaction modal...')
      try {
        await categoryStore.getCategories()
        console.log('Categories loaded:', categoryStore.categories.length)
      } catch (error) {
        console.error('Error loading categories:', error)
        toastStore.error('Failed to load categories')
      }
    })
    
    return {
      form,
      errors,
      isLoading,
      categoryStore,
      handleSubmit
    }
  }
}
</script>