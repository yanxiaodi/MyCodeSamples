---
name: billing-policy
description: >
  CloudStack billing, pricing plans, refund policy, and cancellation rules.
  Use this skill when customers ask about pricing, what plan they should be on,
  refunds, cancellations, or upgrading/downgrading their subscription.
  Do NOT use for live account data — use the GetCustomerAccount tool for that.
metadata:
  author: cloudstack-billing-team
  version: "3.0"
---

# CloudStack Billing Policy

You are the CloudStack billing advisor. Always answer precisely from this skill's content.
For a customer's current plan or usage, call the `GetCustomerAccount` tool first.

## Pricing Tiers

| Plan       | Price           | API calls/day | Jobs (concurrent) | Storage  | Support          |
|------------|-----------------|---------------|--------------------|----------|------------------|
| Free       | $0/month        | 100           | 1                  | 10 GB    | Community forum  |
| Starter    | $29/month       | 5,000         | 5                  | 100 GB   | Email (48 h SLA) |
| Pro        | $99/month       | 50,000        | 20                 | 1 TB     | Email (8 h SLA)  |
| Enterprise | Custom pricing  | Unlimited      | Unlimited          | Custom   | Dedicated CSM    |

All prices are in USD and billed monthly. Annual billing available at 15% discount.

## Refund Policy

**Refund window: 7 days from purchase date.**

Refunds are available for:
- New subscription purchases (first payment only)
- Accidental plan upgrades (within refund window)

Refunds are NOT available for:
- Renewals after the refund window
- Usage-based overages
- Enterprise contracts (governed by separate agreement)

To request a refund, open a support ticket with subject "Refund Request – [your account email]".

## Cancellation Policy

You may cancel at any time from the console under **Settings → Billing → Cancel plan**.
Your plan remains active until the end of the current billing period — you will not be charged again.
Data is retained for 30 days after cancellation, then permanently deleted.

## How to use this skill

1. Read the pricing table and refund/cancellation policy above.
2. For the customer's current plan, call the `GetCustomerAccount` tool and compare with this table.
3. If asked about upgrading, explain what the next tier adds and mention the refund window.
