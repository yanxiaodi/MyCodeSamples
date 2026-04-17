# Troubleshooting: HTTP 429 Too Many Requests

## What it means

You have exceeded the rate limit for your current plan.
The response will include a `Retry-After` header with the number of seconds to wait.

## Rate limits by plan

| Plan       | API calls/day | Burst (calls/minute) |
|------------|---------------|----------------------|
| Free       | 100           | 10                   |
| Starter    | 5,000         | 100                  |
| Pro        | 50,000        | 500                  |
| Enterprise | Unlimited     | Custom               |

## Diagnostic steps

### Step 1 — Read the response headers

```
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1745000000      ← Unix timestamp when limit resets
Retry-After: 3600                  ← Seconds until you can retry
```

### Step 2 — Identify the source of excess calls

Common culprits:
- Polling a resource in a tight loop (use webhooks instead)
- Retrying on every error without back-off
- Multiple instances of your app sharing the same API key

### Step 3 — Fix the call pattern

**Replace polling with webhooks:**

```bash
# Register a webhook (one-time setup)
POST /v2/webhooks
{ "url": "https://yourapp.com/hooks/cloudstack", "events": ["job.completed", "job.failed"] }
```

**Add rate-limit-aware retry logic:**

```python
import time

def handle_response(response):
    if response.status_code == 429:
        retry_after = int(response.headers.get("Retry-After", 60))
        print(f"Rate limited. Waiting {retry_after}s...")
        time.sleep(retry_after)
        return None  # caller should retry
    return response.json()
```

### Step 4 — Consider upgrading your plan

If you legitimately need more calls:
- **Free → Starter**: 50× more calls/day for $29/month
- **Starter → Pro**: 10× more calls/day, 5× burst for $99/month

Use the `GetCustomerAccount` tool to see current usage, then advise accordingly.
