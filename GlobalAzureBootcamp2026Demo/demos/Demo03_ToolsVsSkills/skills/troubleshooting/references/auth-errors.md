# Troubleshooting: Authentication Errors (401 / 403)

## HTTP 401 Unauthorized — "Who are you?"

The request arrived without valid credentials, or the credentials are expired.

### Common causes and fixes

| Cause                          | Symptom                                  | Fix                                              |
|-------------------------------|-------------------------------------------|--------------------------------------------------|
| Missing Authorization header  | `error: "missing_credentials"`           | Add `Authorization: Bearer YOUR_API_KEY`         |
| Key was rotated or deleted    | `error: "invalid_key"`                   | Generate a new key in console → API Keys         |
| Key passed in wrong header    | `error: "invalid_credentials"`           | Use `Authorization: Bearer`, not `X-API-Key`     |
| Expired short-lived token     | `error: "token_expired"`                 | Refresh via `POST /v2/auth/refresh`              |

### Quick check

```bash
# Test your key
curl -I "https://api.cloudstack.io/v2/account/me" \
  -H "Authorization: Bearer YOUR_API_KEY"
```

A 200 response confirms the key is valid.

---

## HTTP 403 Forbidden — "I know who you are, but you can't do this"

The key is valid but lacks the required permission scope for the action.

### Permission scopes

| Action                        | Required scope         |
|-------------------------------|------------------------|
| Read account info             | `account:read`         |
| Submit / read jobs            | `jobs:write`           |
| Read / write storage          | `storage:write`        |
| Manage API keys               | `keys:admin`           |
| Billing and subscription      | `billing:read`         |
| Create support tickets        | `support:write`        |

### How to add a scope

1. Go to **Console → Settings → API Keys**.
2. Click your key → **Edit scopes**.
3. Add the required scope and **Save**.
4. The change takes effect immediately — no need to regenerate the key.

### Service account best practice

Use separate API keys per service, with the minimum required scopes.  
Never use a key with `*` (wildcard) scopes in production.

---

## Still stuck?

If you have verified the key and scopes are correct and still get 401/403, open a support ticket — 
it may indicate an account suspension (error code `CS-2001`) or a platform-side IAM issue.
