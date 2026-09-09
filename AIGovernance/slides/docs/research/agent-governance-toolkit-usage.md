# Microsoft Agent Governance Toolkit：Python `govern()` 与 policy 使用核查

> 研究日期：2026-09-06
>
> 范围：只使用 Microsoft 官方文档站和官方 GitHub repository。本文记录的是当前 `main` 分支和官方文档页面中的用法；Toolkit 标注为 Public Preview，API 可能变化。

## 结论先说

当前官方 Python 高层 API 的真实用法是：

```python
from agentmesh.governance import govern

safe_tool = govern(my_tool, policy="policy.yaml")
safe_tool(action="read")
```

它不是一个只返回自定义 `allow` / `escalate` / `deny` 字符串的示例层。`govern()` 返回 `GovernedCallable`，调用时会在原 callable 执行前做 policy evaluation；默认开启 audit，默认 denial 行为是抛出 `GovernanceDenied`。需要人工审批时，policy action 使用官方 schema 中的 `require_approval`，并通过 `approval_handler` 等参数接入审批流程。

另外，当前仓库的官方页面有一个容易混淆的命名变化：

- 推荐的整体安装：`pip install "agent-governance-toolkit[full]"`
- 只装核心 Python runtime：`pip install "agent-governance-toolkit-core>=5.0.0,<6.0.0"`
- `agentmesh-platform` 在 package consolidation 文档中被列为旧 distribution，迁移目标是 `agent-governance-toolkit-core`。

## 官方来源

### 文档站

- [Agent Governance Toolkit 首页](https://microsoft.github.io/agent-governance-toolkit/)
- [Quick Start](https://microsoft.github.io/agent-governance-toolkit/quickstart/)
- [Tutorial 36: 2-Line Governance with `govern()`](https://microsoft.github.io/agent-governance-toolkit/tutorials/36-govern-quickstart/)
- [Packages](https://microsoft.github.io/agent-governance-toolkit/packages/)
- [AgentMesh package page](https://microsoft.github.io/agent-governance-toolkit/packages/agent-mesh/)

### GitHub source

- [`govern.py`：`govern()` 和 `GovernedCallable`](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/govern.py)
- [`policy.py`：policy model、rule schema 和 evaluator](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/policy.py)
- [`governance/__init__.py`：公开 import](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/__init__.py)
- [`docs/tutorials/36-govern-quickstart.md`](https://github.com/microsoft/agent-governance-toolkit/blob/main/docs/tutorials/36-govern-quickstart.md)
- [`docs/packages/index.md`：当前 Python package 安装说明](https://github.com/microsoft/agent-governance-toolkit/blob/main/docs/packages/index.md)
- [`docs/package-consolidation/MIGRATION.md`：旧 package 到 core 的迁移表](https://github.com/microsoft/agent-governance-toolkit/blob/main/docs/package-consolidation/MIGRATION.md)

## 1. `govern()` 的实际 API

官方 `govern.py` 中的函数签名当前是：

```python
def govern(
    fn: Callable,
    *,
    policy: Union[str, Policy],
    agent_id: str = "*",
    audit: bool = True,
    on_deny: Optional[Callable[[PolicyDecision], Any]] = None,
    approval_handler: Optional[ApprovalHandler] = None,
    advisory: Optional[AdvisoryCheck] = None,
    conflict_strategy: str = "deny_overrides",
    ring: Optional["ExecutionRing"] = None,
    session_id: str = "",
    ...
) -> GovernedCallable:
```

Source: [`govern.py`, `govern()`](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/govern.py#L664-L724)

官方 docstring 明确说明：

```python
from agentmesh.governance import govern

def send_email(to, body):
    ...

safe_send = govern(send_email, policy="email-policy.yaml")
safe_send(to="user@example.com", body="Hello")  # policy-checked
```

`policy` 可以是 policy 文件路径、inline YAML string 或 `Policy` object；`agent_id` 默认是 `"*"`；`audit` 默认是 `True`；`on_deny` 不提供时，denial 默认抛出 `GovernanceDenied`。这些行为都来自同一个官方 `govern()` 实现，而不是 demo 自己定义的约定。

`agentmesh.governance` 包的公开 exports 也直接包含 `govern`、`GovernedCallable`、`GovernanceConfig` 和 `GovernanceDenied`：

```python
from .govern import govern, GovernedCallable, GovernanceConfig, GovernanceDenied
```

Source: [`governance/__init__.py`](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/__init__.py#L9-L22)

## 2. Policy 文件 schema

当前 policy schema 使用：

```yaml
apiVersion: governance.toolkit/v1
```

官方 `Policy` model 的主要顶层字段是：

| 字段 | 类型/默认值 | 含义 |
|---|---|---|
| `apiVersion` | string，默认 `governance.toolkit/v1` | policy schema version |
| `version` | string，默认 `1.0` | policy version metadata |
| `name` | required string | policy 名称 |
| `description` | optional string | policy 描述 |
| `extends` | list[string]，默认空 | 从父 policy 继承规则；官方实现是 additive-only |
| `agent` | optional string | 单个 agent target |
| `agents` | list[string]，默认空 | 多个 agent target；`["*"]` 表示 wildcard |
| `scope` | `global` / `tenant` / `agent`，默认 `global` | policy scope |
| `rules` | list[PolicyRule] | 规则列表 |
| `default_action` | `allow` / `deny`，默认 `deny` | 没有规则匹配时的默认行为 |

Source: [`policy.py`, `Policy`](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/policy.py#L260-L305)

每个 rule 的主要字段是：

| 字段 | 类型/默认值 | 含义 |
|---|---|---|
| `name` | required string | rule 名称 |
| `description` | optional string | 命中时的人类可读原因 |
| `stage` | `pre_input` / `pre_tool` / `post_tool` / `pre_output`，默认 `pre_tool` | lifecycle intervention point |
| `condition` | required string | 条件表达式 |
| `action` | `allow` / `deny` / `warn` / `require_approval` / `log`，默认 `deny` | 命中后的治理动作 |
| `limit` | optional string，例如 `100/hour` | rate limit |
| `approvers` | list[string]，默认空 | `require_approval` 时的 approver 标识 |
| `priority` | integer，默认 `0` | 优先级，数值越高越先处理 |
| `enabled` | boolean，默认 `true` | 是否启用 |

Source: [`policy.py`, `PolicyRule`](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/policy.py#L84-L135)

官方 evaluator 示例支持类似下面的 condition：

```yaml
condition: "action.type == 'export'"
condition: "data.contains_pii"
condition: "user.role in ['admin', 'operator']"
```

官方代码中还明确支持 `and` / `or`、等于/不等于、membership、数值比较和 dot notation；演示时最好使用简单的 `action.type`、`resource.type` 或参数中的布尔/数值字段，避免把表达式语言讲得比官方实现更复杂。

## 3. 官方 policy 示例

来自官方 Tutorial 36 的 policy 片段：

```yaml
apiVersion: governance.toolkit/v1
name: db-access-policy
agents: ["*"]
default_action: allow
rules:
  - name: block-drop
    condition: "action.type == 'drop'"
    action: deny
    description: "DROP operations are never allowed"
    priority: 100

  - name: block-write-to-audit
    condition: "action.type == 'write' and table.value == 'audit_log'"
    action: deny
    description: "Audit log is append-only — no direct writes"
    priority: 100

  - name: require-approval-for-delete
    condition: "action.type == 'delete'"
    action: require_approval
    approvers: ["dba-team"]
    priority: 50
```

Source: [Tutorial 36, Example 1](https://github.com/microsoft/agent-governance-toolkit/blob/main/docs/tutorials/36-govern-quickstart.md#L45-L90)

注意两个容易误用的点：

1. `require_approval` 是官方 policy action；它不是 `escalate`。如果 demo 输出里显示 `escalate`，那只能作为 demo 自己对 `require_approval` 的 UI 映射，不能说是 Toolkit 原生 policy action。
2. 当前 `PolicyDecision.action` 的官方 literal 也是 `allow`、`deny`、`warn`、`require_approval`、`log`；不存在官方的 `escalate` literal。

Source: [`PolicyDecision`](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/policy.py#L498-L536)

## 4. 最小的真实 Python 示例

下面是根据官方 Tutorial 36 的 file-based policy 方式整理出的最小可运行示例。它不是当前仓库某个 demo 文件的逐字复制，但每个 API 和字段都来自官方实现/教程。

### `policy.yaml`

```yaml
apiVersion: governance.toolkit/v1
name: demo-tool-policy
agents: ["*"]
default_action: allow
rules:
  - name: block-delete
    condition: "action.type == 'delete'"
    action: deny
    description: "Delete operations are never allowed"
    priority: 100
```

### `main.py`

```python
from agentmesh.governance import GovernanceDenied, govern


def customer_tool(action="read", **kwargs):
    print(f"TOOL EXECUTED: {action} {kwargs}")
    return {"ok": True, "action": action}


safe_customer_tool = govern(customer_tool, policy="policy.yaml")

safe_customer_tool(action="read", customer_id="C-001")

try:
    safe_customer_tool(action="delete", customer_id="C-001")
except GovernanceDenied as exc:
    print(f"BLOCKED: {exc}")
```

这个例子真正展示了 Toolkit 的关键边界：`customer_tool` 是实际 callable，`safe_customer_tool` 是 Toolkit 返回的 governed wrapper；`delete` 在 policy evaluation 被拒绝后，不会进入 `customer_tool` 的执行体。

官方 Tutorial 36 还展示了 `safe_tool.audit_log.query()` 来查看 wrapper 产生的 audit entries：

```python
for entry in safe_tool.audit_log.query():
    print(f"  {entry.action} → {entry.outcome}")
```

Source: [Tutorial 36, audit example](https://github.com/microsoft/agent-governance-toolkit/blob/main/docs/tutorials/36-govern-quickstart.md#L136-L148)

## 5. `require_approval` 如何接入

官方 wrapper 通过 `approval_handler` 参数处理 `require_approval`。如果没有提供 handler，官方实现使用 `AutoRejectApproval()` 作为默认路径；因此一个真实演示如果想展示“审批后执行”，应明确提供 approval handler，而不是自行把 policy decision 改写成 `escalate`。

官方 API 形态：

```python
safe_tool = govern(
    my_tool,
    policy="policy.yaml",
    approval_handler=my_approval_handler,
)
```

Source: [`govern.py`, approval configuration and handling](https://github.com/microsoft/agent-governance-toolkit/blob/main/agent-governance-python/agent-mesh/src/agentmesh/governance/govern.py#L376-L431) and [Tutorial 36 quick reference](https://github.com/microsoft/agent-governance-toolkit/blob/main/docs/tutorials/36-govern-quickstart.md#L165-L175)

如果 talk demo 要发送 ACS email，建议把 ACS sender 放在被包装的 callable 内，而不是让 policy engine 发送邮件：

```python
def send_email(to, subject, body):
    # ACS SDK call lives here.
    ...


safe_send = govern(
    send_email,
    policy="policy.yaml",
    approval_handler=my_approval_handler,
)
```

Toolkit 负责在 callable 之前作 policy decision；callable 才负责真正的 external side effect。官方首页对这一边界的描述是：ACS 返回 decision，host 负责应用 decision；ACS 本身既不执行 tool，也不保留隐藏的 session state。

Source: [official Toolkit homepage, “ACS is the policy decision layer”](https://microsoft.github.io/agent-governance-toolkit/#acs-is-the-policy-decision-layer)

## 6. 应用到 meetup demo

研究完成后，meetup demo 已按上述真实接口接入 Toolkit。当前实现：

1. `requirements.txt` 安装官方 consolidated `agent-governance-toolkit-core>=5.0.0,<6.0.0`，代码使用官方兼容的 `from agentmesh.governance import govern`。
2. `policy.yaml` 使用官方 schema，并包含 `agents`, `default_action` 和基于 `action.type` 的规则。
3. `governed_agent.py` 用 `govern(actual_callable, policy="policy.yaml")` 包装三个真实工具。
4. wrapper 的 `GovernanceDenied` 控制 tool 是否真正执行；delete 的函数体不会被调用。
5. 人工审批使用 `require_approval` 和 `CallbackApproval`；没有把 Toolkit 的原生 action 称为 `escalate`。
6. audit 菜单通过每个 governed callable 的 `audit_log.query()` 展示 Toolkit 产生的 audit evidence。

demo 的 import 路径保持不变是有意的：package-consolidation 文档要求公共 Python module/class 名保持兼容；demo 只把 distribution 迁移到 `agent-governance-toolkit-core`。
