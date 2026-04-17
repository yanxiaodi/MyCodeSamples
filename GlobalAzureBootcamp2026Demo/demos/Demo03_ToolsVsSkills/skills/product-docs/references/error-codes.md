# CloudStack – Error Code Reference

## HTTP 4xx Client Errors

| Code | Name                  | Meaning                                                    | Fix                                               |
|------|-----------------------|------------------------------------------------------------|---------------------------------------------------|
| 400  | Bad Request           | Missing or invalid fields in the request body             | Check required fields; validate JSON schema       |
| 401  | Unauthorized          | API key missing or expired                                 | Regenerate key in console → Settings → API Keys   |
| 403  | Forbidden             | Key exists but lacks permission for this action            | Add required scope in Settings → API Keys → Scopes|
| 404  | Not Found             | Resource ID doesn't exist in your account                 | Verify the ID; it may have been deleted           |
| 409  | Conflict              | Resource already exists (duplicate name or key collision) | Use a different name, or DELETE the existing resource |
| 422  | Unprocessable Entity  | Request is valid JSON but fails business rules            | Read the `errors[]` array in the response body    |
| 429  | Too Many Requests     | Rate limit exceeded                                        | See the `Retry-After` header; upgrade plan if needed |

## HTTP 5xx Server Errors

| Code | Name                  | Meaning                                                    | Fix                                               |
|------|-----------------------|------------------------------------------------------------|---------------------------------------------------|
| 500  | Internal Server Error | Unexpected server-side error                               | Retry with exponential back-off; open ticket if persistent |
| 502  | Bad Gateway           | Upstream service unreachable                               | Check status.cloudstack.io; retry in 1–2 minutes  |
| 503  | Service Unavailable   | Service temporarily offline (deploy, maintenance, overload)| Check status.cloudstack.io; retry with back-off   |
| 504  | Gateway Timeout       | Request took longer than 30 s                              | Reduce payload size; use async job endpoint       |

## CloudStack application error codes (in `error.code` field)

| Code     | Meaning                                      |
|----------|----------------------------------------------|
| CS-1001  | Plan limit reached (jobs, storage, or calls) |
| CS-1002  | Image not found in registry                  |
| CS-1003  | Job cancelled by user                        |
| CS-2001  | Billing payment failed — account suspended   |
| CS-2002  | Trial expired — upgrade required             |
| CS-3001  | Region not available for your plan           |
