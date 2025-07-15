<template>
  <div>
    <canvas ref="chartCanvas" width="400" height="200"></canvas>
  </div>
</template>

<script>
import { ref, onMounted, watch } from 'vue'

export default {
  name: 'BarChart',
  props: {
    data: {
      type: Object,
      required: true
    }
  },
  setup(props) {
    const chartCanvas = ref(null)
    
    const createChart = () => {
      // For now, just display a placeholder
      const ctx = chartCanvas.value.getContext('2d')
      ctx.fillStyle = '#f8f9fa'
      ctx.fillRect(0, 0, 400, 200)
      ctx.fillStyle = '#6c757d'
      ctx.font = '16px Arial'
      ctx.textAlign = 'center'
      ctx.fillText('Bar Chart Placeholder', 200, 90)
      ctx.fillText('Install Chart.js for real charts', 200, 110)
      
      // Draw simple bars
      if (props.data && props.data.values) {
        ctx.fillStyle = '#e74c3c'
        const barWidth = 40
        const spacing = 50
        const maxValue = Math.max(...props.data.values)
        
        props.data.values.forEach((value, index) => {
          const barHeight = (value / maxValue) * 120
          const x = 50 + index * spacing
          const y = 170 - barHeight
          ctx.fillRect(x, y, barWidth, barHeight)
        })
      }
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