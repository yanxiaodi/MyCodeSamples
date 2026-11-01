<template>
  <svg class="hero-diagram" viewBox="0 0 370 390" role="img" aria-label="Governance controls an agent run; observability records the same run.">
    <defs><pattern id="hero-grid" width="26" height="26" patternUnits="userSpaceOnUse"><circle cx="1" cy="1" r="1" fill="currentColor" opacity=".18"/></pattern></defs>
    <rect width="370" height="390" fill="url(#hero-grid)"/>
    <text x="32" y="48" class="hero-label governance-ink">Governance</text>
    <text x="32" y="70" class="hero-sub">Permission before action</text>
    <path d="M32 95 H312 V157 H280" class="hero-wire governance-wire"/>
    <circle cx="32" cy="95" r="5" class="governance-dot"/>
    <path d="M116 95 V145" class="hero-wire governance-wire"/>
    <rect x="74" y="145" width="222" height="86" rx="12" class="hero-agent"/>
    <path d="M104 172 l-8 8 8 8 M125 172 l8 8 -8 8" fill="none" stroke="var(--cyan)" stroke-width="2"/>
    <text x="151" y="183" class="hero-label">Support agent</text>
    <text x="151" y="207" class="hero-sub">One run. Two questions.</text>
    <path d="M116 231 V288 H326 M255 231 V288" class="hero-wire observation-wire"/>
    <circle cx="116" cy="288" r="5" class="observation-dot"/><circle cx="255" cy="288" r="5" class="observation-dot"/>
    <text x="32" y="336" class="hero-label observation-ink">Observability</text>
    <text x="32" y="359" class="hero-sub">Evidence of execution</text>
    <g class="hero-decision"><rect x="247" y="81" width="62" height="27" rx="5"/><text x="278" y="100" text-anchor="middle">allow</text></g>
    <g class="hero-evidence"><path d="M161 284 v-7 M173 284 v-15 M185 284 v-10 M197 284 v-23 M209 284 v-18" stroke="var(--cyan)" stroke-width="3"/></g>
  </svg>
</template>
