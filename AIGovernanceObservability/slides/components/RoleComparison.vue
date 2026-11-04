<script setup lang="ts">
import { computed } from 'vue'
import { useSlideContext } from '@slidev/client'
import { deletionRuns } from './rehearsal'
const { $clicks, $renderContext } = useSlideContext()
const step = computed(() => $renderContext.value === 'print' ? 3 : Math.min(3, Number($clicks.value)))
const profiles = [
  { label: 'Standard user', permission: 'customer:read', run: deletionRuns.standard },
  { label: 'Support admin', permission: 'customer:read + customer:delete', run: deletionRuns.admin },
]
</script>

<template>
 <div class="role-comparison" :data-stage="step" :data-standard-count="deletionRuns.standard.tool_body_counts.delete_record" :data-admin-count="deletionRuns.admin.tool_body_counts.delete_record">
  <svg class="role-fork" viewBox="0 0 820 32" aria-hidden="true"><path d="M410 0 V12 H205 V32 M410 12 H615 V32"/></svg>
  <div class="role-lanes">
   <div v-for="profile in profiles" :key="profile.run.principal_identity" class="role-lane" :data-trace-id="profile.run.trace_id">
    <div :class="['role-profile', { revealed: step >= 1 }]"><h2>{{ profile.label }}</h2><span>{{ profile.permission }}</span></div>
    <div :class="['role-decision', { revealed: step >= 2 }]"><strong :class="profile.run.policy_decision">Policy: {{ profile.run.policy_decision }}</strong><code>{{ profile.run.rule }}</code></div>
    <div :class="['role-execution', { revealed: step >= 3 }]"><span :class="['role-count', profile.run.policy_decision]">{{ profile.run.tool_body_counts.delete_record }}</span><div>Deletion tool body entries<small>{{ profile.run.policy_decision === 'deny' ? 'Host stops; no executed tool span.' : 'Simulated execution; no record changed.' }}</small></div></div>
   </div>
  </div>
  <div class="diagram-caption"><span>Requested action → caller context → policy → execution evidence</span><span class="build-indicator"><i v-for="n in 4" :key="n" :class="{ active: step >= n - 1 }"/></span></div>
 </div>
</template>
