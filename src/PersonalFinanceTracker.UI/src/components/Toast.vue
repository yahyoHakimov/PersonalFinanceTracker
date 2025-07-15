<template>
  <div class="toast-container position-fixed top-0 end-0 p-3">
    <div
      v-for="toast in toasts"
      :key="toast.id"
      class="toast show"
      role="alert"
      :class="getToastClass(toast.type)"
    >
      <div class="toast-header">
        <i class="bi me-2" :class="getIconClass(toast.type)"></i>
        <strong class="me-auto">{{ getTitle(toast.type) }}</strong>
        <button
          type="button"
          class="btn-close"
          @click="removeToast(toast.id)"
        ></button>
      </div>
      <div class="toast-body">
        {{ toast.message }}
      </div>
    </div>
  </div>
</template>

<script>
import { useToastStore } from '@/stores/toast'

export default {
  name: 'Toast',
  setup() {
    const toastStore = useToastStore()
    
    const getToastClass = (type) => {
      const classes = {
        success: 'text-success',
        error: 'text-danger',
        warning: 'text-warning',
        info: 'text-info'
      }
      return classes[type] || 'text-info'
    }
    
    const getIconClass = (type) => {
      const icons = {
        success: 'bi-check-circle',
        error: 'bi-x-circle',
        warning: 'bi-exclamation-triangle',
        info: 'bi-info-circle'
      }
      return icons[type] || 'bi-info-circle'
    }
    
    const getTitle = (type) => {
      const titles = {
        success: 'Success',
        error: 'Error',
        warning: 'Warning',
        info: 'Info'
      }
      return titles[type] || 'Info'
    }
    
    return {
      toasts: toastStore.toasts,
      removeToast: toastStore.removeToast,
      getToastClass,
      getIconClass,
      getTitle
    }
  }
}
</script>