from fastapi.testclient import TestClient


def test_browser_api_rejects_unknown_scenarios_without_running_tools(monkeypatch):
    monkeypatch.setenv("DEMO_OTLP_ENABLED", "false")
    from app import create_app
    with TestClient(create_app()) as client:
        response = client.post("/api/run", json={
            "scenario": "arbitrary", "identity": "standard", "provider": "mock",
        })
        assert response.status_code == 422
        assert client.get("/api/state").json()["runs"] == []


def test_browser_api_requires_a_supported_simulated_identity(monkeypatch):
    monkeypatch.setenv("DEMO_OTLP_ENABLED", "false")
    from app import create_app
    with TestClient(create_app()) as client:
        for request in [
            {"scenario": "delete", "provider": "mock"},
            {"scenario": "delete", "identity": "owner", "provider": "mock"},
        ]:
            response = client.post("/api/run", json=request)
            assert response.status_code == 422
        assert client.get("/api/state").json()["runs"] == []


def test_browser_api_returns_correlated_denial_and_execution_evidence(monkeypatch):
    monkeypatch.setenv("DEMO_OTLP_ENABLED", "false")
    from app import create_app
    with TestClient(create_app()) as client:
        result = client.post("/api/run", json={
            "scenario": "delete", "identity": "standard", "provider": "mock",
        }).json()
        assert result["outcome"] == "blocked"
        assert result["principal_role"] == "standard_user"
        assert result["tool_body_counts"]["delete_record"] == 0
        state = client.get("/api/state").json()
        assert state["runs"][0]["trace_id"] == result["trace_id"]
        assert any(log["trace_id"] == result["trace_id"] and log["attributes"].get("app.governance.decision") == "deny" for log in state["logs"])
        policy_span = next(
            span for span in state["spans"]
            if span["trace_id"] == result["trace_id"] and span["name"] == "policy.evaluate delete_record"
        )
        assert policy_span["attributes"]["app.demo.identity.role"] == "standard_user"


def test_browser_api_allows_admin_deletion_as_a_simulation(monkeypatch):
    monkeypatch.setenv("DEMO_OTLP_ENABLED", "false")
    from app import create_app
    with TestClient(create_app()) as client:
        result = client.post("/api/run", json={
            "scenario": "delete", "identity": "admin", "provider": "mock",
        }).json()
        assert result["outcome"] == "succeeded"
        assert result["policy_decision"] == "allow"
        assert result["principal_role"] == "support_admin"
        assert result["tool_body_counts"]["delete_record"] == 1
        assert "no customer record was changed" in result["answer"].lower()
        state = client.get("/api/state").json()
        assert any(
            span["trace_id"] == result["trace_id"] and span["name"] == "execute_tool delete_record"
            for span in state["spans"]
        )


def test_missing_foundry_configuration_is_reported_without_mock_fallback(monkeypatch):
    monkeypatch.setenv("DEMO_OTLP_ENABLED", "false")
    for name in ["AZURE_AI_FOUNDRY_ENDPOINT", "AZURE_AI_FOUNDRY_API_KEY", "AZURE_AI_FOUNDRY_DEPLOYMENT"]:
        monkeypatch.delenv(name, raising=False)
    from app import create_app
    with TestClient(create_app()) as client:
        response = client.post("/api/run", json={
            "scenario": "allowed", "identity": "standard", "provider": "foundry",
        })
        assert response.status_code == 503
        assert "Missing Foundry configuration" in response.json()["detail"]
        assert client.get("/api/state").json()["runs"] == []
