from __future__ import annotations

import json
import os
import sys
import tempfile
from pathlib import Path

from fastapi.testclient import TestClient

ROOT = Path(__file__).resolve().parents[2]
SERVER_ROOT = ROOT / "server"
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))


def build_runtime_proof() -> dict[str, object]:
    os.environ.setdefault("KBBQ_DB_PATH", str(Path(tempfile.gettempdir()) / "kbbq_architecture_pack.sqlite"))
    os.environ.setdefault("KBBQ_HMAC_SECRET", "architecture-pack-secret")
    os.environ.setdefault("KBBQ_TOKEN_SALT", "architecture-pack-salt")
    os.environ.setdefault("KBBQ_MAX_CLOCK_SKEW_SECONDS", "9999")

    from server.app import app

    with TestClient(app) as client:
        health = client.get("/health")
        meta = client.get("/meta")
        architecture_pack = client.get("/architecture-pack")
        readiness = client.get("/readiness")

    for response in (health, meta, architecture_pack, readiness):
        response.raise_for_status()

    architecture_pack_payload = architecture_pack.json()
    return {
        "service": "kbbq-idle-backend",
        "health": health.json().get("diagnostics", {}),
        "meta_contract": meta.json().get("architecture_pack_contract"),
        "architecture_pack_contract": architecture_pack_payload.get("readiness_contract"),
        "proof_bundle": architecture_pack_payload.get("proof_bundle", {}),
        "review_routes": architecture_pack_payload.get("links", {}),
        "readiness": readiness.json(),
    }


def main() -> None:
    output_path = None
    args = iter(sys.argv[1:])
    for arg in args:
        if arg == "--output":
            candidate = next(args, "")
            output_path = Path(candidate) if candidate else None

    rendered = json.dumps(build_runtime_proof(), ensure_ascii=True, indent=2)
    if output_path is not None:
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_text(f"{rendered}\n", encoding="utf-8")
    print(rendered)


if __name__ == "__main__":
    main()
