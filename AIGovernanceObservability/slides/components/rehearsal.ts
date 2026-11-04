import session from '../../docs/evidence/session.json'

export const rehearsal = session
export const runs = session.runs.filter(run => run.provider === 'mock')
export const deletionRuns = {
  standard: runs.find(run => run.scenario === 'delete' && run.principal_identity === 'standard')!,
  admin: runs.find(run => run.scenario === 'delete' && run.principal_identity === 'admin')!,
}
export const counts = {
  allow: runs.filter(run => run.policy_decision === 'allow').length,
  deny: runs.filter(run => run.policy_decision === 'deny').length,
  succeeded: runs.filter(run => run.outcome === 'succeeded').length,
  blocked: runs.filter(run => run.outcome === 'blocked').length,
  failed: runs.filter(run => run.outcome === 'failed').length,
  tokens: runs.reduce((sum, run) => sum + run.input_tokens + run.output_tokens, 0),
}
export function traceFor(scenario: string) {
  const run = runs.find(run => run.scenario === scenario)!
  const spans = session.spans.filter(span => span.trace_id === run.trace_id)
  const root = spans.find(span => span.name === 'invoke_agent support-agent')!
  return { run, root, spans: spans.filter(span => span.span_id !== root.span_id)
    .sort((a, b) => a.start_time_ms - b.start_time_ms) }
}
