<template>
  <div class="modal fade show d-block" style="background-color: rgba(0,0,0,0.5)">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">Add Category</h5>
          <button type="button" class="btn-close" @click="$emit('close')"></button>
        </div>
        
        <form @submit.prevent="handleSubmit">
          <div class="modal-body">
            <div class="mb-3">
              <label for="name" class="form-label">Name</label>
              <input
                type="text"
                class="form-control"
                id="name"
                v-model="form.name"
                :class="{ 'is-invalid': errors.name }"
                required
                maxlength="100"
                placeholder="Enter category name"
              />
              <div v-if="errors.name" class="invalid-feedback">
                {{ errors.name }}
              </div>
              <small class="text-muted">Only letters, numbers, spaces, hyphens and underscores allowed</small>
            </div>
            
            <div class="mb-3">
              <label for="color" class="form-label">Color</label>
              <div class="d-flex align-items-center">
                <input
                  type="color"
                  class="form-control form-control-color me-2"
                  id="color"
                  v-model="form.color"
                  :class="{ 'is-invalid': errors.color }"
                  style="width: 60px; height: 38px;"
                />
                <input
                  type="text"
                  class="form-control"
                  v-model="form.color"
                  :class="{ 'is-invalid': errors.color }"
                  placeholder="#000000"
                  pattern="^#[0-9A-Fa-f]{6}$"
                />
              </div>
              <div v-if="errors.color" class="invalid-feedback">
                {{ errors.color }}
              </div>
              <small class="text-muted">Hex color format (e.g., #FF0000)</small>
            </div>
            
            <!-- Color Preview -->
            <div class="mb-3">
              <label class="form-label">Preview</label>
              <div class="d-flex align-items-center">
                <span 
                  class="badge me-2 px-3 py-2" 
                  :style="{ 
                    backgroundColor: form.color, 
                    color: getTextColor(form.color),
                    fontSize: '14px'
                  }"
                >
                  {{ form.name || 'Category Name' }}
                </span>
              </div>
            </div>
          </div>
          
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="$emit('close')">
              Cancel
            </button>
            <button type="submit" class="btn btn-primary" :disabled="isLoading">
              <span v-if="isLoading" class="loading-spinner me-2"></span>
              {{ isLoading ? 'Saving...' : 'Save Category' }}
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
  name: 'AddCategoryModal',
  emits: ['close', 'category-added'],
  setup(props, { emit }) {
    const categoryStore = useCategoryStore()
    const toastStore = useToastStore()
    
    const form = reactive({
      name: '',
      color: '#' + Math.floor(Math.random()*16777215).toString(16) // Random color
    })
    
    const errors = ref({})
    const isLoading = ref(false)
    
    const getTextColor = (backgroundColor) => {
      if (!backgroundColor) return '#000000'
      const hex = backgroundColor.replace('#', '')
      const r = parseInt(hex.substr(0, 2), 16) || 0
      const g = parseInt(hex.substr(2, 2), 16) || 0
      const b = parseInt(hex.substr(4, 2), 16) || 0
      const brightness = (r * 299 + g * 587 + b * 114) / 1000
      return brightness > 155 ? '#000000' : '#ffffff'
    }
    
    const handleSubmit = async () => {
      errors.value = {}
      
      // Client-side validation matching your server validators
      if (!form.name.trim()) {
        errors.value.name = 'Category name is required'
      } else if (form.name.length < 1 || form.name.length > 100) {
        errors.value.name = 'Category name must be between 1 and 100 characters'
      } else if (!/^[a-zA-Z0-9\s\-_]+$/.test(form.name)) {
        errors.value.name = 'Category name can only contain letters, numbers, spaces, hyphens and underscores'
      }
      
      if (!form.color) {
        errors.value.color = 'Category color is required'
      } else if (!/^#[0-9A-Fa-f]{6}$/.test(form.color)) {
        errors.value.color = 'Category color must be a valid hex color (e.g., #FF0000)'
      }
      
      if (Object.keys(errors.value).length > 0) {
        return
      }
      
      try {
        isLoading.value = true
        
        const result = await categoryStore.createCategory({
          name: form.name.trim(),
          color: form.color.toUpperCase()
        })
        
        if (result.success) {
          toastStore.success('Category added successfully!')
          emit('category-added')
          emit('close')
        } else {
          toastStore.error(result.message || 'Failed to add category')
        }
      } catch (error) {
        console.error('Category creation error:', error)
        toastStore.error('Failed to add category')
      } finally {
        isLoading.value = false
      }
    }
    
    return {
      form,
      errors,
      isLoading,
      getTextColor,
      handleSubmit
    }
  }
}
</script>