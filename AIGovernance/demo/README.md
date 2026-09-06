# Governed agent demo

This is the deterministic local demo used by the meetup talk. It uses the
official Microsoft Agent Governance Toolkit Python API to demonstrate the
action boundary with three tools:

- `lookup_customer` → allow and execute
- `send_email` → `require_approval`, ask for approval through the toolkit's
  `CallbackApproval`, then send through Azure Communication Services when configured
- `delete_record` → `deny`; the toolkit wrapper raises `GovernanceDenied` and
  the tool function is never called

The important integration is in `governed_agent.py`:

```python
from agentmesh.governance import govern

safe_send = govern(send_email, policy="policy.yaml", approval_handler=handler)
```

The dependency uses the consolidated `agent-governance-toolkit-core` package.
The `agentmesh.governance` Python module is intentionally retained because the
official package-consolidation migration guarantees public import compatibility.

`policy.yaml` uses the toolkit's `apiVersion: governance.toolkit/v1` schema.
The audit menu prints entries from each governed callable's
`safe_tool.audit_log`, rather than from a demo-only audit list.

Open this `demo` directory as the VS Code workspace and run:

```powershell
python -m pip install -r requirements.txt
python governed_agent.py
```

The menu displays the complete request sent to the agent. It runs one action
at a time: choose `1`, `2`, or `3` to demonstrate an allow, approval, or
denial. Approval is requested only after the agent actually selects
`send_email` and AGT evaluates the `require_approval` policy. Choose `4` to
show the audit events accumulated so far, or `q` to exit. For a
non-interactive rehearsal, use for example:

```powershell
python governed_agent.py --once email
```

The non-interactive email path is denied by default. Add `--approve-email`
only when you intentionally want to approve it:

```powershell
python governed_agent.py --once email --approve-email
```

The default `auto` provider uses Foundry first when all Foundry variables are
configured, and falls back to the local mock if the model request fails. If no
Foundry variables are configured, it uses the mock directly. Copy
`.env.example` to `.env` inside this `demo` directory, fill in the Azure AI
Foundry values, and run:

`AZURE_AI_FOUNDRY_ENDPOINT` should use the OpenAI-compatible v1 base URL,
for example `https://<resource>.services.ai.azure.com/openai/v1`. The deployment
name is passed as the `model` value to the Responses API.

```powershell
python governed_agent.py --provider foundry
```

The `.env` file can also contain
`AZURE_COMMUNICATION_SERVICES_CONNECTION_STRING`,
`AZURE_COMMUNICATION_SERVICES_SENDER`, and `DEMO_EMAIL_RECIPIENT`. The current
demo does not send email or make external calls by default; the real ACS sender
runs only after the toolkit policy decision and an explicit approval.
