<script setup lang="ts">
import { computed } from 'vue'
import { useSlideContext } from '@slidev/client'
const props = withDefaults(defineProps<{ scenario?: 'compare' | 'allowed' | 'blocked' | 'failed', static?: boolean }>(), { scenario: 'compare', static: false })
const { $clicks, $renderContext } = useSlideContext()
const step = computed(() => props.static || $renderContext.value === 'print' ? 3 : Math.min(3, Number($clicks.value)))
const isCompare = computed(() => props.scenario === 'compare')
const isBlocked = computed(() => props.scenario === 'blocked')
const isFailed = computed(() => props.scenario === 'failed')
const stages = ['A native tool request', 'Validate and evaluate policy', 'Read the decision', 'The host enforces the result']
</script>

<template>
 <div class="agent-flow" :data-stage="step" :data-scenario="scenario">
  <svg viewBox="0 0 830 275" role="img" :aria-label="isCompare ? 'Tool request is evaluated by policy. The host either enters the tool body or stops execution.' : scenario + ' scenario: model request, policy decision and host result.'">
   <g class="flow-source">
    <rect x="16" y="78" width="179" height="99" rx="10" class="flow-node"/>
    <path d="M40 103 l-7 8 7 8 M57 103 l7 8 -7 8" stroke="var(--cyan)" fill="none" stroke-width="2"/>
    <text x="39" y="142" class="diagram-title">Model requests</text>
    <text x="39" y="162" class="diagram-small">{{ isBlocked ? 'delete_record' : isCompare ? 'tool name + arguments' : 'lookup_customer' }}</text>
   </g>
   <g class="diagram-reveal" :class="{ revealed: step >= 1 }">
    <path d="M195 126 H285" class="flow-link cyan-link"/>
    <path d="m277 120 8 6 -8 6" class="flow-arrow cyan-link"/>
    <rect x="286" y="78" width="224" height="99" rx="10" class="flow-node policy-node"/>
    <text x="310" y="110" class="diagram-small governance-ink">Agent Governance Toolkit</text>
    <text x="310" y="140" class="diagram-title">Policy evaluates</text>
    <text x="310" y="162" class="diagram-small">Action + context + rules</text>
   </g>
   <g class="diagram-reveal" :class="{ revealed: step >= 2 }">
    <rect x="330" y="190" width="134" height="27" rx="5" :class="['decision-tag', isBlocked ? 'tag-deny' : 'tag-allow']"/>
    <text x="397" y="208" text-anchor="middle" :class="['diagram-tag', isBlocked ? 'deny' : 'allow']">{{ isCompare ? 'allow or deny' : isBlocked ? 'deny' : 'allow' }}</text>
   </g>
   <g class="diagram-reveal" :class="{ revealed: step >= 3 }">
    <template v-if="isCompare">
     <path d="M510 111 H565 V58 H634" class="flow-link allow-link"/>
     <path d="M510 145 H565 V216 H634" class="flow-link deny-link"/>
     <rect x="635" y="20" width="179" height="80" rx="10" class="flow-node allow-node"/>
     <text x="657" y="51" class="diagram-title allow">Enter tool body</text><text x="657" y="76" class="diagram-small">Allowed request</text>
     <rect x="635" y="179" width="179" height="80" rx="10" class="flow-node deny-node"/>
     <text x="657" y="210" class="diagram-title deny">Stop execution</text><text x="657" y="235" class="diagram-small">Denied request</text>
     <text x="567" y="45" class="diagram-tag allow">allow</text><text x="567" y="201" class="diagram-tag deny">deny</text>
    </template>
    <template v-else>
     <path d="M510 126 H634" :class="['flow-link', isBlocked ? 'deny-link' : 'allow-link']"/>
     <path d="m626 120 8 6 -8 6" :class="['flow-arrow', isBlocked ? 'deny-link' : 'allow-link']"/>
     <rect x="635" y="78" width="179" height="99" rx="10" :class="['flow-node', isBlocked ? 'deny-node' : isFailed ? 'fail-node' : 'allow-node']"/>
     <text x="657" y="115" :class="['diagram-title', isBlocked ? 'deny' : isFailed ? 'fail' : 'allow']">{{ isBlocked ? 'Host stops' : 'Host executes' }}</text>
     <text x="657" y="141" class="diagram-small">{{ isBlocked ? 'No tool entry' : 'Lookup body entered' }}</text>
     <text x="657" y="163" :class="['diagram-tag', isBlocked ? 'deny' : isFailed ? 'fail' : 'allow']">{{ isBlocked ? 'blocked' : isFailed ? 'then: TimeoutError' : 'succeeded' }}</text>
    </template>
   </g>
  </svg>
  <div class="diagram-caption"><span>{{ stages[step] }}</span><span v-if="!static" class="build-indicator"><i v-for="n in 4" :key="n" :class="{ active: step >= n - 1 }"/></span></div>
 </div>
</template>
