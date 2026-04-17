# Troubleshooting: HTTP 503 Service Unavailable

## What it means

The CloudStack API is temporarily unable to handle requests. Common causes:

1. **Planned maintenance** — scheduled window in progress
2. **Unplanned incident** — service overloaded or a dependency is down
3. **Deployment rollout** — brief unavailability during blue-green deploy (usually < 30 s)

## Diagnostic steps

### Step 1 — Check the status page

Always check https://status.cloudstack.io first.
If an incident is active, subscribe to updates — no further action needed until resolved.

Also use the `GetServiceHealth` tool to see the live status and any active incident message.

### Step 2 — Confirm it's not your network

```bash
curl -I https://api.cloudstack.io/v2/health
```

- HTTP 200 → API is up; the 503 may have been transient.
- HTTP 503 → Confirmed outage; follow Step 3.
- Connection refused / timeout → Check your own network / firewall / DNS.

### Step 3 — Implement retry with exponential back-off

Do NOT hammer the API — it makes recovery slower. Use exponential back-off:

```python
import time, random, cloudstack

def call_with_retry(fn, max_retries=5):
    for attempt in range(max_retries):
        try:
            return fn()
        except cloudstack.ServiceUnavailableError:
            if attempt == max_retries - 1:
                raise
            wait = (2 ** attempt) + random.uniform(0, 1)
            print(f"503 — retrying in {wait:.1f}s (attempt {attempt + 1}/{max_retries})")
            time.sleep(wait)
```

Recommended back-off: 1 s, 2 s, 4 s, 8 s, 16 s.

### Step 4 — Use async jobs for long-running operations

If a specific endpoint consistently times out, switch to the async job pattern:

```bash
# Submit async job
POST /v2/jobs  →  returns { "jobId": "job_xyz" }

# Poll for result
GET /v2/jobs/job_xyz  →  { "status": "completed", "result": {...} }
```

## When to open a ticket

Open a support ticket if:
- The 503 persists for more than 15 minutes with no active status-page incident
- You are on Pro or Enterprise and need SLA credits
- You need a post-incident report (Enterprise only)
