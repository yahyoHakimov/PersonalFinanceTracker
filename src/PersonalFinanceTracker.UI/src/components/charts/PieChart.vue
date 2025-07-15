<template>
  <div>
    <canvas ref="chartCanvas" width="400" height="200"></canvas>
  </div>
</template>

<script>
import { ref, onMounted, watch } from 'vue'
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

export default {
  name: 'PieChart',
  props: {
    data: {
      type: Object,
      required: true
    }
  },
  setup(props) {
    const chartCanvas = ref(null)
    let chart = null
    
    const createChart = () => {
      if (chart) {
        chart.destroy()
      }
      
      const ctx = chartCanvas.value.getContext('2d')
      
      chart = new Chart(ctx, {
        type: 'pie',
        data: {
          labels: props.data.labels,
          datasets: [{
            data: props.data.values,
            backgroundColor: [
              '#e74c3c',
              '#3498db',
              '#f39c12',
              '#27ae60',
              '#9b59b6',
              '#e67e22',
              '#1abc9c',
              '#34495e'
            ]
          }]
        },
        options: {
          responsive: true,
          plugins: {
            legend: {
              position: 'bottom'
            }
          }
        }
      })
    }
    
    onMounted(() => {
      createChart()
    })
    
    watch(() => props.data, () => {
      createChart()
    }, { deep: true })
    
    return {
      chartCanvas
    }
  }
}
</script>