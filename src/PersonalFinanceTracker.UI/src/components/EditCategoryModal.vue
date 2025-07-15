<template>
  <div class="modal fade show d-block" style="background-color: rgba(0,0,0,0.5)">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">Edit Category</h5>
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
                  id="income-cat-edit"
                  value="income"
                  v-model="form.type"
                >
                <label class="btn btn-outline-success" for="income-cat-edit">Income</label>
                
                <input
                  type="radio"
                  class="btn-check"
                  name="type"
                  id="expense-cat-edit"
                  value="expense"
                  v-model="form.type"
                >
                <label class="btn btn-outline-danger" for="expense-cat-edit">Expense</label>
              </div>
            </div>
            
            <div class="mb-3">
              <label for="name-edit" class="form-label">Name</label>
              <input
                type="text"
                class="form-control"
                id="name-edit"
                v-model="form.name"
                :class="{ 'is-invalid': errors.name }"
                required
              />
              <div v-if="errors.name" class="invalid-feedback">
                {{ errors.name }}
              </div>
            </div>
            
            <div class="mb-3">
              <label for="description-edit" class="form-label">Description (Optional)</label>
              <textarea
                class="form-control"
                id="description-edit"
                rows="3"
                v-model="form.description"
              ></textarea>
            </div>
          </div>
          
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="$emit('close')">
              Cancel
            </button>
            <button type="submit" class="btn btn-primary" :disabled="isLoading">
              <span v-if="isLoading" class="loading-spinner me-2"></span>
              {{ isLoading ? 'Updating...' : 'Update Category' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive } from 'vue'
import { useCategoryStore } from '@/stores/category'
import { useToastStore } from '@/stores/toast'

export default {
  name: 'EditCategoryModal',
  props: {
    category: {
      type: Object,
      required: true
    }
  },
  emits: ['close', 'category-updated'],
  setup(props, { emit }) {
    const categoryStore = useCategoryStore()
    const toastStore = useToastStore()
    
    const form = reactive({
      type: props.category.type,
      name: props.category.name,
      description: props.category.description || ''
    })
    
    const errors = ref({})
    const isLoading = ref(false)
    
    const handleSubmit = async () => {
      errors.value = {}
      
      // Basic validation
      if (!form.name.trim()) {
        errors.value.name = 'Name is required'
      }
      
      if (Object.keys(errors.value).length > 0) {
        return
      }
      
      try {
        isLoading.value = true
        
        const result = await categoryStore.updateCategory(props.category.id, form)
        
        if (result.success) {
          toastStore.success('Category updated successfully!')
          emit('category-updated')
          emit('close')
        } else {
          toastStore.error(result.message || 'Failed to update category')
        }
      } catch (error) {
        toastStore.error('Failed to update category')
      } finally {
        isLoading.value = false
      }
    }
    
    return {
      form,
      errors,
      isLoading,
      handleSubmit
    }
  }
}
</script>
