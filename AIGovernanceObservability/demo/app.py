"""Local browser controls. All scenarios call the same governed agent."""
import os
import threading
from collections import deque
from contextlib import asynccontextmanager
from pathlib import Path
from typing import Literal

from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
from pydantic import BaseModel
from starlette.concurrency import run_in_threadpool

from support_agent.agent import AgentRunner, DEMO_PRINCIPALS, DemoIdentity
from support_agent.providers import create_provider, ModelProviderError
from support_agent.telemetry import configure_telemetry

DEMO_ROOT = Path(__file__).resolve().parent


class RunRequest(BaseModel):
    scenario: Literal["allowed", "delete", "failed"]
    identity: DemoIdentity
    provider: Literal["mock", "foundry"] = "mock"


def create_app():
    history = deque(maxlen=50)
    history_lock = threading.Lock()

    @asynccontextmanager
    async def lifespan(app):
        load_dotenv(os.getenv("DEMO_ENV_FILE", str(DEMO_ROOT / ".env")), override=False)
        enabled = os.getenv("DEMO_OTLP_ENABLED", "true").lower() == "true"
        app.state.telemetry = configure_telemetry(
            otlp_endpoint=os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:43118") if enabled else None,
            artifact_path=DEMO_ROOT / "artifacts" / "traces.jsonl" if enabled else None,
        )
        yield
        await run_in_threadpool(app.state.telemetry.flush)

    app = FastAPI(title="Governed support agent", lifespan=lifespan)

    @app.get("/")
    def home():
        return FileResponse(DEMO_ROOT / "index.html")

    def execute(request):
        provider = create_provider(request.provider)
        try:
            result = AgentRunner(provider).run(
                request.scenario, principal=DEMO_PRINCIPALS[request.identity],
            )
        finally:
            provider.close()
        app.state.telemetry.flush()
        with history_lock:
            history.appendleft(result.to_dict())
        return result.to_dict()

    @app.post("/api/run")
    async def run(request: RunRequest):
        try:
            return await run_in_threadpool(execute, request)
        except ModelProviderError as exc:
            raise HTTPException(status_code=503, detail=str(exc)) from None

    @app.get("/api/state")
    def state():
        telemetry = app.state.telemetry
        with history_lock:
            runs = list(history)
        return {"runs": runs, "spans": telemetry.spans(), "logs": telemetry.logs(),
                "metrics": telemetry.metric_points(),
                "dashboard_url": os.getenv("DEMO_DASHBOARD_URL", "http://localhost:18988"),
                "otlp_enabled": bool(telemetry.otlp_endpoint)}

    return app


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(create_app(), host="127.0.0.1", port=int(os.getenv("DEMO_PORT", "8090")))
