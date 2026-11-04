<script setup lang="ts">
import { computed } from 'vue'
import { useSlideContext } from '@slidev/client'
const { $clicks, $renderContext } = useSlideContext()
const step = computed(() => $renderContext.value === 'print' ? 3 : Math.min(3, Number($clicks.value)))
</script>

<template>
 <div class="telemetry-map" :data-stage="step">
  <svg viewBox="0 0 830 315" role="img" aria-label="OpenLLMetry instruments model calls. The application records policy and tools. OTel providers export traces, logs and metrics over OTLP to Aspire.">
   <rect x="10" y="12" width="574" height="131" rx="12" class="host-envelope"/>
   <text x="30" y="39" class="diagram-small">Python support agent</text>
   <rect x="304" y="19" width="260" height="28" rx="5" class="policy-node flow-node"/>
   <text x="434" y="38" text-anchor="middle" class="diagram-small governance-ink">Host-selected simulated caller</text>
   <path d="M420 47 V55 H288 V63" class="flow-link" stroke="var(--amber)"/>
   <g v-for="(name,i) in ['Model SDK','AGT policy','Local tools']" :key="name">
    <rect :x="30+i*183" y="63" width="150" height="60" rx="7" class="flow-node"/>
    <text :x="105+i*183" y="99" text-anchor="middle" class="diagram-title">{{ name }}</text>
    <path v-if="i<2" :d="'M'+(180+i*183)+' 93 h29'" class="flow-link muted-link"/>
   </g>
   <g class="diagram-reveal" :class="{ revealed: step >= 1 }">
    <path d="M105 124 V174 M288 124 V174 M471 124 V174" class="flow-link cyan-link"/>
    <text x="105" y="195" text-anchor="middle" class="diagram-title observation-ink">OpenLLMetry</text>
    <text x="105" y="217" text-anchor="middle" class="diagram-small">Model spans + metrics</text>
    <text x="383" y="195" text-anchor="middle" class="diagram-title observation-ink">Application instrumentation</text>
    <text x="383" y="217" text-anchor="middle" class="diagram-small">Policy + role + tool + run evidence</text>
   </g>
   <g class="diagram-reveal" :class="{ revealed: step >= 2 }">
    <path d="M105 228 V272 H260 M383 228 V246" class="flow-link cyan-link"/>
    <rect x="260" y="247" width="270" height="52" rx="8" class="provider-node"/>
    <text x="395" y="278" text-anchor="middle" class="diagram-title">OTel trace / log / metric providers</text>
   </g>
   <g class="diagram-reveal" :class="{ revealed: step >= 3 }">
    <path d="M530 272 H589 V172 H638" class="flow-link cyan-link"/>
    <text x="587" y="254" class="diagram-tag observation-ink">OTLP</text>
    <rect x="638" y="70" width="180" height="160" rx="12" class="viewer-node"/>
    <text x="661" y="102" class="diagram-title">Aspire dashboard</text>
    <path d="M661 127 h45 M661 137 h28 M661 147 h36" stroke="var(--cyan)" stroke-width="3"/>
    <text x="721" y="142" class="diagram-small">Traces</text>
    <path d="M661 169 h45 M661 180 h45" stroke="var(--cyan)" stroke-width="2" stroke-dasharray="4 3"/>
    <text x="721" y="178" class="diagram-small">Logs</text>
    <path d="M662 210 v-13 M675 210 v-25 M688 210 v-18 M701 210 v-31" stroke="var(--cyan)" stroke-width="4"/>
    <text x="721" y="207" class="diagram-small">Metrics</text>
   </g>
  </svg>
  <div class="diagram-caption"><span>Instrumentation collects. OpenTelemetry exports. Aspire visualizes.</span></div>
 </div>
</template>
