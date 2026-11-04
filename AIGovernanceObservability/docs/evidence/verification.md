# Verification: 4 November 2026

- Python: 20 demo tests passed with `DEMO_ENV_FILE` set to a nonexistent path,
  isolating the tests from local environment configuration.
- Browser: the same deletion request was denied for the standard user and allowed
  for the support admin. The admin path ran only the no-op simulated deletion.
- Slidev production build and PDF export succeeded. `talk.pdf` has 15 pages,
  each 720 x 404.88 pt; pdf-lib successfully parsed the exported PDF.
- Browser console: one missing `/favicon.ico` returned 404; the app API requests
  used for the demo succeeded.

## Recorded behavior

`session.json` captures 11 offline runs: nine warm-up runs (three each for
`allowed`, `delete`, and `failed`) plus the two manual role checks. Ten runs used
the standard-user profile and one used the support-admin profile. The capture has
44 recorded spans and 32 correlated logs. Policy decisions: 7 allow, 4 deny.
Outcomes: 4 succeeded, 4 blocked, and 3 failed. The offline token fixtures total
1,440 input and 326 output tokens.

| Simulated role | Decision | Matched rule | Outcome | Lookup/deletion body counts | Trace ID |
| --- | --- | --- | --- | --- | --- |
| Standard user | deny | `deny-non-admin-deletion` | blocked | 0 / 0 | 9561f82e59027019cfed0702cd2284d2 |
| Support admin | allow | `allow-admin-deletion` | succeeded (simulated) | 0 / 1 | b169d374301f75dd71d5f4808d8091a1 |

Across all 11 runs, `lookup_customer` executed six times and `delete_record`
executed once, for the authorized admin simulation. No customer record changed.
No Foundry model call was made in this verification; responses and token counts
came from the deterministic offline provider.

## Aspire

This browser capture used `DEMO_OTLP_ENABLED=false`, so `session.json` contains
in-process evidence and does not represent a new Aspire run. The existing Aspire
screenshots and OTLP checks are from the earlier verification and were not
repeated for the role-aware flow.

## Limits

The login selector maps to two fixed demo profiles; it is not an identity
provider or a complete authentication system. Records are synthetic, and the
deletion tool is a no-op simulation. Prompt/tool payload capture is off. Traces
and metrics are diagnostic evidence, not durable auditing or proof of response
quality.
