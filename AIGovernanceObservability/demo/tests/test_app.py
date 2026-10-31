from fastapi.testclient import TestClient


def test_browser_api_rejects_unknown_scenarios_without_running_tools(monkeypatch):
    monkeypatch.setenv("DEMO_OTLP_ENABLED", "false")
    from app import create_app
    with TestClient(create_app()) as client:
        response = client.post("/api/run", json={"scenario": "arbitrary", "provider": "mock"})
        assert response.status_code == 422
        assert client.get("/api/state").json()["runs"] == []


def test_browser_api_returns_correlated_denial_and_execution_evidence(monkeypatch):
    monkeypatch.setenv("DEMO_OTLP_ENABLED", "false")
    from app import create_app
    with TestClient(create_app()) as client:
        result = client.post("/api/run", json={"scenario": "blocked", "provider": "mock"}).json()
        assert result["outcome"] == "blocked"
        assert result["tool_body_counts"]["delete_record"] == 0
        state = client.get("/api/state").json()
        assert state["runs"][0]["trace_id"] == result["trace_id"]
        assert any(log["trace_id"] == result["trace_id"] and log["attributes"].get("app.governance.decision") == "deny" for log in state["logs"])


def test_missing_foundry_configuration_is_reported_without_mock_fallback(monkeypatch):
    monkeypatch.setenv("DEMO_OTLP_ENABLED", "false")
    for name in ["AZURE_AI_FOUNDRY_ENDPOINT", "AZURE_AI_FOUNDRY_API_KEY", "AZURE_AI_FOUNDRY_DEPLOYMENT"]:
        monkeypatch.delenv(name, raising=False)
    from app import create_app
    with TestClient(create_app()) as client:
        response = client.post("/api/run", json={"scenario": "allowed", "provider": "foundry"})
        assert response.status_code == 503
        assert "Missing Foundry configuration" in response.json()["detail"]
        assert client.get("/api/state").json()["runs"] == []
