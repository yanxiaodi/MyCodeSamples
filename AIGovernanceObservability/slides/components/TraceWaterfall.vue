<script setup lang="ts">
import { computed } from 'vue'
import { useSlideContext } from '@slidev/client'
import { traceFor } from './rehearsal'
const props = withDefaults(defineProps<{ scenario?: string, static?: boolean, compact?: boolean }>(), { scenario: 'failed', static: false, compact: false })
const { $clicks, $renderContext } = useSlideContext()
const step = computed(() => props.static || $renderContext.value === 'print' ? 4 : Math.min(4, Number($clicks.value)))
const data = computed(() => traceFor(props.scenario))
const rows = computed(() => [data.value.root, ...data.value.spans])
const limit = computed(() => Math.ceil(data.value.root.duration_ms / 250) * 250)
const x = (time: number) => 324 + time / limit.value * 389
const shortName = (name: string) => name.replace(' support-agent', '').replace(' lookup_customer', '')
const rowReady = (index: number) => index === 0 || step.value >= Math.min(index, 3)
const timeLabel = (time: number) => time < 1 ? '< 1 ms' : Math.round(time) + ' ms'
const toolShare = computed(() => Math.round(100 * data.value.spans.find(s => s.name.startsWith('execute_tool'))!.duration_ms / data.value.root.duration_ms))
</script>

<template>
 <div :class="['waterfall', { compact }]" :data-stage="step" :data-trace-id="data.run.trace_id">
  <div class="waterfall-head"><span>One trace · {{ scenario === 'failed' ? 'injected timeout' : 'allowed lookup' }}</span><strong>{{ Math.round(data.root.duration_ms) }} ms</strong></div>
  <svg :viewBox="'0 0 810 ' + (rows.length * 51 + 46)" role="img" :aria-label="'Recorded ' + scenario + ' trace: ' + Math.round(data.root.duration_ms) + ' milliseconds total.'">
   <g v-for="tick in 5" :key="tick" class="timeline-grid">
    <path :d="'M' + x((tick-1)*limit/4) + ' 27 V' + (rows.length*51+25)"/>
    <text :x="x((tick-1)*limit/4)" y="18" text-anchor="middle">{{ (tick-1)*limit/4 }}{{ tick === 5 ? ' ms' : '' }}</text>
   </g>
   <g v-for="(row,index) in rows" :key="row.span_id" :class="['waterfall-row', { revealed: rowReady(index), 'tool-highlight': step >= 4 && row.name.startsWith('execute_tool') }]">
    <rect x="9" :y="index*51+30" width="790" height="45" rx="4" class="row-shade"/>
    <path v-if="index" :d="'M19 ' + (index*51+44) + ' v11 h10'" class="parent-mark"/>
    <text :x="index ? 40 : 20" :y="index*51+58" class="waterfall-name">{{ shortName(row.name) }}</text>
    <rect :x="x(row.start_time_ms-data.root.start_time_ms)" :y="index*51+42" :width="rowReady(index) ? Math.max(3,row.duration_ms/limit*389) : 0" height="18" rx="3" :class="['waterfall-bar', { root: !index, failed: row.status === 'ERROR', policy: row.name.startsWith('policy.') }]"/>
    <text x="794" :y="index*51+57" text-anchor="end" class="waterfall-time">{{ timeLabel(row.duration_ms) }}</text>
   </g>
  </svg>
  <div v-if="scenario === 'failed'" :class="['waterfall-insight', { revealed: step >= 4 }]"><strong>{{ toolShare }}%</strong><span>of this run was in the tool body.<br><b>Policy allowed it. The tool timed out.</b></span></div>
  <div class="trace-footnote">Recorded offline rehearsal. Short spans use a minimum-width marker; durations are printed.</div>
 </div>
</template>
