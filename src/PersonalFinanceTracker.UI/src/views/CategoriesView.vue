<template>
  <div>
    <div class="row mb-4">
      <div class="col-md-6">
        <h1>Categories</h1>
      </div>
      <div class="col-md-6 text-end">
        <button class="btn btn-primary" @click="showAddModal = true">
          <i class="bi bi-plus"></i> Add Category
        </button>
      </div>
    </div>
    
    <!-- DEBUG SECTION - Remove after testing -->
    <div class="alert alert-info mb-4">
      <h6>DEBUG INFO:</h6>
      <p><strong>Loading:</strong> {{ categoryStore.isLoading }}</p>
      <p><strong>Total Categories:</strong> {{ categoryStore.categories.length }}</p>
      <button class="btn btn-sm btn-primary me-2" @click="debugLoadCategories">Force Reload</button>
      <button class="btn btn-sm btn-info" @click="showRawData = !showRawData">Toggle Raw Data</button>
      
      <div v-if="showRawData" class="mt-2">
        <strong>Raw Categories Data:</strong>
        <pre>{{ JSON.stringify(categoryStore.categories, null, 2) }}</pre>
      </div>
    </div>
    
    <!-- Categories List - Show all categories (no type filtering) -->
    <div class="card">
      <div class="card-header">
        <h5>All Categories ({{ categoryStore.categories.length }})</h5>
      </div>
      <div class="card-body">
        <div v-if="categoryStore.isLoading" class="text-center py-4">
          <div class="loading-spinner"></div>
          <p class="mt-2">Loading categories...</p>
        </div>
        
        <div v-else-if="categoryStore.categories.length === 0" class="text-center py-4">
          <p class="text-muted">No categories found</p>
          <button class="btn btn-primary" @click="showAddModal = true">
            Add Your First Category
          </button>
        </div>
        
        <div v-else class="row">
          <div
            v-for="category in categoryStore.categories"
            :key="category.id"
            class="col-md-6 col-lg-4 mb-3"
          >
            <div class="card h-100">
              <div class="card-body">
                <div class="d-flex justify-content-between align-items-start mb-2">
                  <h6 class="card-title mb-1">{{ category.name }}</h6>
                  <div>
                    <button
                      class="btn btn-sm btn-outline-primary me-1"
                      @click="editCategory(category)"
                    >
                      <i class="bi bi-pencil"></i>
                    </button>
                    <button
                      class="btn btn-sm btn-outline-danger"
                      @click="deleteCategory(category)"
                    >
                      <i class="bi bi-trash"></i>
                    </button>
                  </div>
                </div>
                
                <div class="mb-2">
                  <span 
                    class="badge me-2" 
                    :style="{ 
                      backgroundColor: category.color || '#000000', 
                      color: getTextColor(category.color || '#000000') 
                    }"
                  >
                    {{ category.color || '#000000' }}
                  </span>
                </div>
                
                <div class="row text-center">
                  <div class="col-6">
                    <small class="text-muted">Transactions</small>
                    <div class="fw-bold">{{ category.transactionCount || 0 }}</div>
                  </div>
                  <div class="col-6">
                    <small class="text-muted">Total Amount</small>
                    <div class="fw-bold">${{ formatCurrency(category.totalAmount || 0) }}</div>
                  </div>
                </div>
                
                <small class="text-muted d-block mt-2">
                  Created: {{ formatDate(category.createdAt) }}
                </small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Add Category Modal -->
    <AddCategoryModal
      v-if="showAddModal"
      @close="showAddModal = false"
      @category-added="refreshCategories"
    />
    
    <!-- Edit Category Modal -->
    <EditCategoryModal
      v-if="showEditModal"
      :category="selectedCategory"
      @close="showEditModal = false"
      @category-updated="refreshCategories"
    />
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { useCategoryStore } from '@/stores/category'
import { useToastStore } from '@/stores/toast'
import AddCategoryModal from '@/components/AddCategoryModal.vue'
import EditCategoryModal from '@/components/EditCategoryModal.vue'

export default {
  name: 'CategoriesView',
  components: {
    AddCategoryModal,
    EditCategoryModal
  },
  setup() {
    const categoryStore = useCategoryStore()
    const toastStore = useToastStore()
    
    const showAddModal = ref(false)
    const showEditModal = ref(false)
    const selectedCategory = ref(null)
    const showRawData = ref(false)
    
    const getTextColor = (backgroundColor) => {
      if (!backgroundColor) return '#000000'
      const hex = backgroundColor.replace('#', '')
      const r = parseInt(hex.substr(0, 2), 16)
      const g = parseInt(hex.substr(2, 2), 16)
      const b = parseInt(hex.substr(4, 2), 16)
      const brightness = (r * 299 + g * 587 + b * 114) / 1000
      return brightness > 155 ? '#000000' : '#ffffff'
    }
    
    const formatCurrency = (amount) => {
      return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      }).format(amount)
    }
    
    const formatDate = (dateString) => {
      if (!dateString) return 'Unknown'
      return new Date(dateString).toLocaleDateString()
    }
    
    const debugLoadCategories = async () => {
      console.log('Debug: Force loading categories...')
      try {
        await categoryStore.getCategories()
        console.log('Debug: Categories loaded:', categoryStore.categories)
        toastStore.success(`Loaded ${categoryStore.categories.length} categories`)
      } catch (error) {
        console.error('Debug: Failed to load categories:', error)
        toastStore.error('Failed to load categories: ' + error.message)
      }
    }
    
    const editCategory = (category) => {
      selectedCategory.value = category
      showEditModal.value = true
    }
    
    const deleteCategory = async (category) => {
      if (confirm(`Are you sure you want to delete the category "${category.name}"?`)) {
        try {
          await categoryStore.deleteCategory(category.id)
          toastStore.success('Category deleted successfully!')
        } catch (error) {
          console.error('Delete category error:', error)
          toastStore.error('Failed to delete category')
        }
      }
    }
    
    const refreshCategories = () => {
      debugLoadCategories()
    }
    
    onMounted(() => {
      console.log('CategoriesView mounted, loading categories...')
      debugLoadCategories()
    })
    
    return {
      categoryStore,
      showAddModal,
      showEditModal,
      selectedCategory,
      showRawData,
      getTextColor,
      formatCurrency,
      formatDate,
      debugLoadCategories,
      editCategory,
      deleteCategory,
      refreshCategories
    }
  }
}
</script>