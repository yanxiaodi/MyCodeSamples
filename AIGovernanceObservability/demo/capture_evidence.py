"""Save synthetic rehearsal evidence from the local demo, without configuration."""
import json
from pathlib import Path

import httpx


def main():
    response = httpx.get("http://127.0.0.1:8090/api/state", timeout=10)
    response.raise_for_status()
    data = response.json()
    path = Path(__file__).resolve().parents[1] / "docs" / "evidence" / "session.json"
    path.write_text(json.dumps({"note": "Synthetic rehearsal data; offline tokens are fixtures.",
                                "runs": data["runs"], "spans": data["spans"],
                                "logs": data["logs"], "metrics": data["metrics"]}, indent=2), encoding="utf-8")
    print(f"Recorded {len(data['runs'])} runs to {path}")


if __name__ == "__main__":
    main()
