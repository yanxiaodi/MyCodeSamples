"""Rehearse one scenario or a small labelled batch without a browser."""
import argparse
import json
import os
from pathlib import Path

from dotenv import load_dotenv

from support_agent.agent import AgentRunner
from support_agent.providers import create_provider, ModelProviderError
from support_agent.telemetry import configure_telemetry


def main():
    parser = argparse.ArgumentParser(description="Governance and observability demo")
    parser.add_argument("--scenario", choices=["allowed", "blocked", "failed", "all"], default="all")
    parser.add_argument("--provider", choices=["mock", "foundry"], default="mock")
    parser.add_argument("--warmup", type=int, default=1, help="Number of batches, from 1 to 10; each is labelled demo traffic.")
    parser.add_argument("--env-file", type=Path, help="Read configuration from this file without changing it.")
    parser.add_argument("--no-otlp", action="store_true", help="Collect local evidence only, without a running dashboard.")
    args = parser.parse_args()
    if not 1 <= args.warmup <= 10:
        parser.error("--warmup must be between 1 and 10")
    root = Path(__file__).resolve().parent
    load_dotenv(args.env_file or root / ".env", override=False)
    telemetry = configure_telemetry(
        otlp_endpoint=None if args.no_otlp else os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:43118"),
        artifact_path=root / "artifacts" / "traces.jsonl",
    )
    try:
        provider = create_provider(args.provider)
    except ModelProviderError as exc:
        print(str(exc))
        return 1
    failed = False
    try:
        runner = AgentRunner(provider)
        scenarios = ["allowed", "blocked", "failed"] if args.scenario == "all" else [args.scenario]
        for _ in range(args.warmup):
            for scenario in scenarios:
                result = runner.run(scenario)
                print(json.dumps(result.to_dict(), indent=2))
                failed |= result.error_type not in {"", "TimeoutError"}
    finally:
        provider.close()
        telemetry.flush()
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
