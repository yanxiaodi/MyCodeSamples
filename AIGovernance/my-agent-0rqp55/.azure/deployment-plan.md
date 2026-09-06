# Deployment Plan

Status: Ready for Validation

## Scope

Add optional Azure Application Insights instrumentation to the existing Microsoft Foundry Hosted Agent governance demo.

## Decisions

- Keep the existing Foundry Hosted Agent deployment shape.
- Configure Azure Monitor OpenTelemetry only when `APPLICATIONINSIGHTS_CONNECTION_STRING` is present.
- Add non-sensitive structured tool-call logs for the demo tools.
- Pass the connection string through `azure.yaml` environment variables.

## Validation

- Run the local unit tests and Python compilation checks.
- Validate the dependency/import path where the local environment permits.
- Deployment itself is outside this change.

## Verification status

- Unit tests: passed (3 tests).
- Source compilation: passed without writing bytecode.
- Policy manifest YAML: parsed successfully and contains all three tools.
- Full Windows dependency installation: not completed because the ACS package
  resolves to a source distribution on Windows and requires Rust/Cargo; the
  Foundry Hosted Agent target is Linux, where the published wheel is available.
