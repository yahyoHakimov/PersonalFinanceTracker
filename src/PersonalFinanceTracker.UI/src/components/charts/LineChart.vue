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
  name: 'LineChart',
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
        type: 'line',
        data: {
          labels: props.data.labels,
          datasets: [
            {
              label: 'Income',
              data: props.data.income,
              borderColor: '#27ae60',
              backgroundColor: 'rgba(39, 174, 96, 0.1)',
              tension: 0.4
            },
            {
              label: 'Expenses',
              data: props.data.expenses,
              borderColor: '#e74c3c',
              backgroundColor: 'rgba(231, 76, 60, 0.1)',
              tension: 0.4
            }
          ]
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true
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
