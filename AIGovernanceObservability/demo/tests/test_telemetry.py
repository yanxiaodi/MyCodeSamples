from concurrent.futures import ThreadPoolExecutor


def runtime():
    from support_agent.telemetry import configure_telemetry
    return configure_telemetry(otlp_endpoint=None, artifact_path=None)


def test_model_policy_and_tool_spans_share_one_trace():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    telemetry = runtime()
    result = AgentRunner(MockProvider()).run("allowed", principal=DEMO_PRINCIPALS["standard"])
    telemetry.flush()
    spans = [s for s in telemetry.spans() if s["trace_id"] == result.trace_id]
    assert len(result.trace_id) == 32 and int(result.trace_id, 16) != 0
    root = next(s for s in spans if s["name"] == "invoke_agent support-agent")
    policy = next(s for s in spans if s["name"] == "policy.evaluate lookup_customer")
    tool = next(s for s in spans if s["name"] == "execute_tool lookup_customer")
    models = [s for s in spans if s["scope"].startswith("opentelemetry.instrumentation.openai")]
    assert len(models) == 2
    assert policy["parent_span_id"] == root["span_id"]
    assert policy["attributes"]["app.demo.identity.role"] == "standard_user"
    assert tool["parent_span_id"] == root["span_id"]
    assert all(s["parent_span_id"] == root["span_id"] for s in models)


def test_blocked_run_has_policy_deny_but_no_executed_tool_span():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    telemetry = runtime()
    result = AgentRunner(MockProvider()).run("delete", principal=DEMO_PRINCIPALS["standard"])
    telemetry.flush()
    spans = [s for s in telemetry.spans() if s["trace_id"] == result.trace_id]
    policy = next(s for s in spans if s["name"] == "policy.evaluate delete_record")
    assert policy["attributes"]["app.governance.decision"] == "deny"
    assert policy["attributes"]["app.demo.identity.role"] == "standard_user"
    assert policy["status"] != "ERROR"
    assert not any(s["name"].startswith("execute_tool") for s in spans)


def test_admin_delete_trace_records_allow_and_simulated_execution():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    telemetry = runtime()
    result = AgentRunner(MockProvider()).run("delete", principal=DEMO_PRINCIPALS["admin"])
    telemetry.flush()
    spans = [s for s in telemetry.spans() if s["trace_id"] == result.trace_id]
    policy = next(s for s in spans if s["name"] == "policy.evaluate delete_record")
    tool = next(s for s in spans if s["name"] == "execute_tool delete_record")
    assert policy["attributes"]["app.governance.decision"] == "allow"
    assert policy["attributes"]["app.governance.rule"] == "allow-admin-deletion"
    assert policy["attributes"]["app.demo.identity.role"] == "support_admin"
    assert tool["status"] != "ERROR"


def test_metrics_include_model_tokens_policy_decisions_and_runtime_outcomes():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    telemetry = runtime()
    AgentRunner(MockProvider()).run("allowed", principal=DEMO_PRINCIPALS["standard"])
    AgentRunner(MockProvider(), timeout_seconds=0.01).run(
        "failed", principal=DEMO_PRINCIPALS["standard"],
    )
    AgentRunner(MockProvider()).run("delete", principal=DEMO_PRINCIPALS["standard"])
    AgentRunner(MockProvider()).run("delete", principal=DEMO_PRINCIPALS["admin"])
    points = telemetry.metric_points()
    names = {p["name"] for p in points}
    assert "gen_ai.client.token.usage" in names
    assert "app.governance.decisions" in names
    assert "app.agent.runs" in names
    assert "app.tool.duration" in names
    decisions = [p for p in points if p["name"] == "app.governance.decisions"]
    assert any(p["attributes"].get("app.governance.decision") == "deny" and p["value"] >= 1 for p in decisions)
    runs = [p for p in points if p["name"] == "app.agent.runs"]
    assert {"succeeded", "blocked", "failed"} <= {p["attributes"]["app.run.outcome"] for p in runs}


def test_default_telemetry_excludes_messages_customer_ids_and_tool_arguments():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    telemetry = runtime()
    result = AgentRunner(MockProvider()).run("allowed", principal=DEMO_PRINCIPALS["standard"])
    telemetry.flush()
    spans = [s for s in telemetry.spans() if s["trace_id"] == result.trace_id]
    import json
    captured = json.dumps(spans)
    assert "C-1042" not in captured
    assert "Avery Example" not in captured
    assert "gen_ai.input.messages" not in captured
    assert "gen_ai.output.messages" not in captured


def test_concurrent_runs_keep_execution_counts_and_trace_ids_separate():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    runtime()
    with ThreadPoolExecutor(max_workers=2) as pool:
        results = list(pool.map(
            lambda run: AgentRunner(MockProvider()).run(run[0], principal=DEMO_PRINCIPALS[run[1]]),
            [("allowed", "standard"), ("delete", "admin")],
        ))
    assert results[0].trace_id != results[1].trace_id
    assert results[0].tool_body_counts == {"lookup_customer": 1, "delete_record": 0}
    assert results[1].tool_body_counts == {"lookup_customer": 0, "delete_record": 1}
    assert results[0].principal_role == "standard_user"
    assert results[1].principal_role == "support_admin"


def test_dashboard_polling_does_not_evict_agent_evidence():
    from opentelemetry import trace
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    telemetry = runtime()
    result = AgentRunner(MockProvider()).run("allowed", principal=DEMO_PRINCIPALS["standard"])
    for _ in range(510):
        with trace.get_tracer("opentelemetry.instrumentation.fastapi").start_as_current_span("GET /api/state"):
            pass
    telemetry.flush()
    assert any(s["trace_id"] == result.trace_id and s["name"] == "invoke_agent support-agent"
               for s in telemetry.spans())
