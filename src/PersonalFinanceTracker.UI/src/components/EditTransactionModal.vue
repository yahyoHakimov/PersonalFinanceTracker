<template>
  <div class="modal fade show d-block" style="background-color: rgba(0,0,0,0.5)">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">Edit Transaction</h5>
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
                  id="income-edit"
                  value="income"
                  v-model="form.type"
                >
                <label class="btn btn-outline-success" for="income-edit">Income</label>
                
                <input
                  type="radio"
                  class="btn-check"
                  name="type"
                  id="expense-edit"
                  value="expense"
                  v-model="form.type"
                >
                <label class="btn btn-outline-danger" for="expense-edit">Expense</label>
              </div>
            </div>
            
            <div class="mb-3">
              <label for="description-edit" class="form-label">Description</label>
              <input
                type="text"
                class="form-control"
                id="description-edit"
                v-model="form.description"
                :class="{ 'is-invalid': errors.description }"
                required
              />
              <div v-if="errors.description" class="invalid-feedback">
                {{ errors.description }}
              </div>
            </div>
            
            <div class="mb-3">
              <label for="amount-edit" class="form-label">Amount</label>
              <input
                type="number"
                step="0.01"
                class="form-control"
                id="amount-edit"
                v-model="form.amount"
                :class="{ 'is-invalid': errors.amount }"
                required
              />
              <div v-if="errors.amount" class="invalid-feedback">
                {{ errors.amount }}
              </div>
            </div>
            
            <div class="mb-3">
              <label for="categoryId-edit" class="form-label">Category</label>
              <select
                class="form-select"
                id="categoryId-edit"
                v-model="form.categoryId"
                :class="{ 'is-invalid': errors.categoryId }"
                required
              >
                <option value="">Select a category</option>
                <option
                  v-for="category in filteredCategories"
                  :key="category.id"
                  :value="category.id"
                >
                  {{ category.name }}
                </option>
              </select>
              <div v-if="errors.categoryId" class="invalid-feedback">
                {{ errors.categoryId }}
              </div>
            </div>
            
            <div class="mb-3">
              <label for="date-edit" class="form-label">Date</label>
              <input
                type="date"
                class="form-control"
                id="date-edit"
                v-model="form.date"
                :class="{ 'is-invalid': errors.date }"
                required
              />
              <div v-if="errors.date" class="invalid-feedback">
                {{ errors.date }}
              </div>
            </div>
            
            <div class="mb-3">
              <label for="notes-edit" class="form-label">Notes (Optional)</label>
              <textarea
                class="form-control"
                id="notes-edit"
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
              {{ isLoading ? 'Updating...' : 'Update Transaction' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive, computed, onMounted } from 'vue'
import { useTransactionStore } from '@/stores/transaction'
import { useToastStore } from '@/stores/toast'

export default {
  name: 'EditTransactionModal',
  props: {
    transaction: {
      type: Object,
      required: true
    }
  },
  emits: ['close', 'transaction-updated'],
  setup(props, { emit }) {
    const transactionStore = useTransactionStore()
    const toastStore = useToastStore()
    
    const form = reactive({
      type: props.transaction.type,
      description: props.transaction.description,
      amount: props.transaction.amount,
      categoryId: props.transaction.categoryId,
      date: new Date(props.transaction.date).toISOString().split('T')[0],
      notes: props.transaction.notes || ''
    })
    
    const errors = ref({})
    const isLoading = ref(false)
    
    const filteredCategories = computed(() => {
      return transactionStore.categories.filter(category => 
        category.type === form.type
      )
    })
    
    const handleSubmit = async () => {
      errors.value = {}
      
      // Basic validation
      if (!form.description.trim()) {
        errors.value.description = 'Description is required'
      }
      
      if (!form.amount || form.amount <= 0) {
        errors.value.amount = 'Amount must be greater than 0'
      }
      
      if (!form.categoryId) {
        errors.value.categoryId = 'Category is required'
      }
      
      if (!form.date) {
        errors.value.date = 'Date is required'
      }
      
      if (Object.keys(errors.value).length > 0) {
        return
      }
      
      try {
        isLoading.value = true
        
        const result = await transactionStore.updateTransaction(props.transaction.id, {
          ...form,
          amount: parseFloat(form.amount)
        })
        
        if (result.success) {
          toastStore.success('Transaction updated successfully!')
          emit('transaction-updated')
          emit('close')
        } else {
          toastStore.error(result.message || 'Failed to update transaction')
        }
      } catch (error) {
        toastStore.error('Failed to update transaction')
      } finally {
        isLoading.value = false
      }
    }
    
    onMounted(() => {
      if (transactionStore.categories.length === 0) {
        transactionStore.getCategories()
      }
    })
    
    return {
      form,
      errors,
      isLoading,
      filteredCategories,
      handleSubmit
    }
  }
}
</script>