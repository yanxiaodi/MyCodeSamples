<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { Chart, BarController, BarElement, CategoryScale, LinearScale, Tooltip } from 'chart.js'
import { useSlideContext } from '@slidev/client'
import { counts, runs } from './rehearsal'
Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip)
const { $clicks, $renderContext, $nav, $page } = useSlideContext()
const stage = computed(() => $renderContext.value === 'print' ? 3 : Math.min(3, Number($clicks.value)))
const canvas = ref<HTMLCanvasElement>()
let chart: Chart | undefined
let reduced = false
const colors = ['#7de0a3','#ff7d8b','#7de0a3','#ff7d8b','#f3bd67']
const actual = [[counts.allow,0],[counts.deny,0],[0,counts.succeeded],[0,counts.blocked],[0,counts.failed]]
const visible = () => actual.map(values => [stage.value >= 1 ? values[0] : 0, stage.value >= 2 ? values[1] : 0])
const labels = ['Allow','Deny','Succeeded','Blocked','Failed']
const countLabels = {
 id: 'rehearsal-count-labels',
 afterDatasetsDraw(instance: Chart) {
  const ctx = instance.ctx
  ctx.save(); ctx.fillStyle='#07131c'; ctx.font='600 16px Aptos, sans-serif'; ctx.textAlign='center'; ctx.textBaseline='middle'
  instance.data.datasets.forEach((dataset, index) => instance.getDatasetMeta(index).data.forEach((bar: any, i) => {
   if (Number(dataset.data[i]) > 0 && Math.abs(bar.x-bar.base) > 20) ctx.fillText(String(dataset.data[i]), (bar.x+bar.base)/2, bar.y)
  }))
  ctx.restore()
 }
}
function update() {
 if (!chart) return
 const data = visible()
 chart.data.datasets.forEach((dataset,i) => dataset.data = data[i])
 chart.options.animation = reduced || $renderContext.value === 'print' ? false : { duration: 650 }
 chart.update(reduced || $renderContext.value === 'print' ? 'none' : undefined)
}
onMounted(() => {
 reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches
 chart = new Chart(canvas.value!, {
  type: 'bar',
  data: { labels: ['Policy decision','Run outcome'], datasets: labels.map((label,i) => ({label,data:visible()[i],backgroundColor:colors[i],borderRadius:3,barThickness:26})) },
  plugins: [countLabels],
  options: {
   indexAxis:'y', responsive:true, maintainAspectRatio:false,
   animation: reduced || $renderContext.value === 'print' ? false : {duration:650},
   layout: {padding:{top:5,right:7,bottom:0}},
   plugins:{tooltip:{enabled:true}},
   scales:{
    x:{stacked:true,min:0,max:runs.length,grid:{color:'#294350'},border:{display:false},ticks:{stepSize:3,color:'#a4bdc8',font:{size:12}}},
    y:{stacked:true,grid:{display:false},border:{display:false},ticks:{color:'#e8f3f7',font:{family:'Aptos, sans-serif',size:14}}}
   }
  }
 })
})
watch([stage, () => $nav.value.currentSlideNo], async () => {
 await nextTick()
 if (chart && ($nav.value.currentSlideNo === $page.value || $renderContext.value === 'print')) chart.resize()
 update()
})
onBeforeUnmount(() => chart?.destroy())
</script>

<template>
 <div class="outcome-dashboard" :data-stage="stage" :data-policy-allow="counts.allow" :data-policy-deny="counts.deny" :data-runtime-success="counts.succeeded" :data-runtime-blocked="counts.blocked" :data-runtime-failed="counts.failed">
  <div class="chart-heading"><h2>{{ runs.length }} runs. Two different outcome counts.</h2><span>Recorded offline rehearsal</span></div>
  <div class="outcome-canvas"><canvas ref="canvas" role="img" :aria-label="'Policy: '+counts.allow+' allow, '+counts.deny+' deny. Runs: '+counts.succeeded+' succeeded, '+counts.blocked+' blocked, '+counts.failed+' failed.'">Policy allows: {{ counts.allow }}. Policy denials: {{ counts.deny }}. Succeeded: {{ counts.succeeded }}. Blocked: {{ counts.blocked }}. Failed: {{ counts.failed }}.</canvas></div>
  <div :class="['chart-legend', { revealed: stage >= 1 }]"><span class="allow">Allow {{ counts.allow }}</span><span class="deny">Deny {{ counts.deny }}</span></div>
  <div :class="['chart-legend', { revealed: stage >= 2 }]"><span class="allow">Succeeded {{ counts.succeeded }}</span><span class="deny">Blocked {{ counts.blocked }}</span><span class="fail">Failed {{ counts.failed }}</span></div>
  <div :class="['chart-conclusion', { revealed: stage >= 3 }]"><strong>{{ counts.allow }} allowed requests ≠ {{ counts.succeeded }} successful runs</strong><span>{{ counts.tokens.toLocaleString() }} synthetic input + output tokens</span></div>
 </div>
</template>
