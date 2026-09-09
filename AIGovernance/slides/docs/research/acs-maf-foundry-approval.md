# ACS、MAF 与 Foundry Hosted Agent Approval 核查

> 研究日期：2026-09-06
>
> 只记录 Microsoft Agent Governance Toolkit、Microsoft Agent Framework、Azure SDK 和 Microsoft Learn 的官方资料。

## 结论

### `agent-control-specification` 已可直接从 PyPI 安装

当前 AGT 官方文档和 PyPI 都支持：

```bash
pip install agent-control-specification
```

当前 PyPI 版本为 `0.3.1b1`，要求 Python `>=3.11`。发布文件包括 Linux x86_64 的预编译 wheel 和 source distribution；当前发布列表没有 Windows wheel，因此 Windows 环境可能会退回 source distribution 并需要本地 Rust/maturin 构建。

这修正了之前把“从 AGT 仓库源码安装”说成唯一安装方式的结论。源码安装仍然是 AGT Tutorial 55 描述的仓库开发路径，但不是当前唯一方式。

来源：

- https://microsoft.github.io/agent-governance-toolkit/packages/agent-control-specification/
- https://microsoft.github.io/agent-governance-toolkit/quickstart/
- https://microsoft.github.io/agent-governance-toolkit/tutorials/55-agent-control-specification/
- https://pypi.org/project/agent-control-specification/

### `ResponsesHostServer` 已经做了 MAF approval 到 Responses approval 的协议映射

Microsoft Agent Framework 的 Hosted Agent 实现中，`ResponsesHostServer` 会：

1. 接收 Agent Framework 的 `function_approval_request`；
2. 将它输出为 Responses 协议的 `mcp_approval_request`；
3. 保存 approval request，供后续请求恢复；
4. 接收客户端的 `mcp_approval_response`；
5. 将其转换为 Agent Framework 的 function approval response。

因此，对于 `@tool(approval_mode="always_require")` 产生的工具审批，请求链路已经能映射到 Hosted Agent 的 Responses endpoint，不需要自己再写一套 approval endpoint。

来源：

- https://github.com/microsoft/agent-framework/blob/main/python/packages/foundry_hosting/agent_framework_foundry_hosting/_responses.py
- https://github.com/microsoft/agent-framework/blob/main/python/packages/foundry_hosting/README.md
- https://learn.microsoft.com/en-us/agent-framework/hosting/foundry-hosted-agent
- https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/responses

Azure Responses API 的官方 approval 流程也是：返回 `mcp_approval_request`，客户端再用 `mcp_approval_response` 和 `previous_response_id` 继续请求。

### Foundry Portal 自带界面是否一定显示按钮

协议层已经支持 approval request/response；Hosted Agent 也会持久化 pending approval。但官方资料没有明确承诺 Foundry Portal Playground 对由 MAF 本地工具产生的 approval request 一定显示 Approve/Reject 按钮。

所以要实际用用户当前的 Foundry 界面做一次 smoke test。可以确认的是：

- Responses API 客户端可以处理该 approval 流程；
- Agent Framework DevUI 明确有 approval UI；
- Foundry Portal Playground 是否将这类请求渲染为按钮，需要以当前租户和版本的实际行为为准。

### ACS 的 `escalate` 不等于 MAF 的 function approval

ACS 的 `escalate` 是 policy verdict，表示 host 需要调用 approval backend 或暂停等待 host 恢复。当前 AGT MAF adapter 的 `CapabilityGuardMiddleware` 主要处理 allow、deny 和 transform；不能假设 ACS `escalate` 会自动变成 MAF `FunctionApprovalRequestContent`。

因此 demo 需要验证或实现这一层桥接：

```text
ACS policy: escalate
       ↓
MAF approval request
       ↓
ResponsesHostServer: mcp_approval_request
       ↓
Foundry client UI
```

如果当前 adapter 尚未提供直接桥接，短期可靠方案是让 `send_email` 使用 MAF 的 `approval_mode="always_require"` 产生结构化审批，同时让 ACS 负责审计和 allow/deny；长期方案是把 ACS 的 `escalate` resolver 接到 MAF approval lifecycle，避免两个独立审批机制重复弹窗。

来源：

- https://github.com/microsoft/agent-governance-toolkit/blob/main/examples/maf-integration/README.md
- https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-os/src/agent_os/integrations/maf_adapter.py
- https://microsoft.github.io/agent-governance-toolkit/tutorials/38-approval-workflows/
