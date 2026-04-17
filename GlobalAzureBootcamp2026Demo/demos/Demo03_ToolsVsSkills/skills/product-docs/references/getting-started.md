# CloudStack – Getting Started

## Step 1: Create an account

Go to https://console.cloudstack.io and sign up with your email.
You will start on the **Free** plan — no credit card required.

## Step 2: Generate an API key

1. Open **Settings → API Keys** in the console.
2. Click **Generate new key**.
3. Copy both the **Key ID** and the **Secret** — the secret is shown only once.

## Step 3: Make your first API call

```bash
curl -X GET "https://api.cloudstack.io/v2/account/me" \
  -H "Authorization: Bearer YOUR_API_KEY"
```

Expected response:

```json
{
  "id": "acct_abc123",
  "email": "you@example.com",
  "plan": "Free",
  "apiCallsThisMonth": 0
}
```

## Step 4: Submit your first compute job

```bash
curl -X POST "https://api.cloudstack.io/v2/jobs" \
  -H "Authorization: Bearer YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{"image": "cloudstack/python:3.12", "command": ["python", "-c", "print(\"hello\")"]}'
```

## SDK support

| Language | Package          | Install                        |
|----------|------------------|--------------------------------|
| Python   | cloudstack-sdk   | `pip install cloudstack-sdk`   |
| Node.js  | @cloudstack/sdk  | `npm install @cloudstack/sdk`  |
| .NET     | CloudStack.SDK   | `dotnet add package CloudStack.SDK` |
| Go       | cloudstack-go    | `go get github.com/cloudstack/cloudstack-go` |

## Rate limits (Free plan)

- 100 API calls / day
- 1 concurrent job
- 10 GB storage

Upgrade to **Starter** or higher to increase limits.
