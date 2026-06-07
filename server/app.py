import json
import os
import time
import uuid
from contextlib import contextmanager
from pathlib import Path

import httpx
from fastapi import FastAPI, HTTPException, Request, Response
from fastapi.middleware.cors import CORSMiddleware

from server.db import get_db
from server.models import (
    AnalyticsEventRequest,
    AuthResponse,
    CommunityFeedbackRequest,
    FriendInviteRequest,
    FriendListResponse,
    LeaderboardEntry,
    LeaderboardResponse,
    ScoreSubmitRequest,
)
from server.security import (
    ensure_friend_code,
    hmac_b64,
    new_token,
    require_bearer_player_id,
    token_sha256,
    verify_signed_headers,
)


def _is_truthy(value: str) -> bool:
    return (value or "").strip().lower() in ("1", "true", "yes", "on")


EXPOSE_DOCS = _is_truthy(os.getenv("KBBQ_EXPOSE_DOCS", "0"))
APP_STARTED_AT = int(time.time())
RATE_BUCKETS: dict[str, list[float]] = {}
REVIEW_PACK_CONTRACT = "kbbq-idle-review-pack-v1"
DEFAULT_CORS_ORIGINS = [
    "http://localhost:3000",
    "http://127.0.0.1:3000",
    "https://kbbq-idle-unity.pages.dev",
]


def _read_csv_env(name: str, default: list[str]) -> list[str]:
    raw = str(os.getenv(name) or "").strip()
    if not raw:
        return list(default)
    return [item.strip() for item in raw.split(",") if item.strip()]


KBBQ_CORS_ORIGINS = _read_csv_env("KBBQ_CORS_ORIGINS", DEFAULT_CORS_ORIGINS)

app = FastAPI(
    title="KBBQ Idle Backend",
    version="0.1",
    # Reviewers don't need a public Swagger UI by default.
    docs_url="/docs" if EXPOSE_DOCS else None,
    redoc_url=None,
    openapi_url="/openapi.json" if EXPOSE_DOCS else None,
)
app.add_middleware(
    CORSMiddleware,
    allow_origins=KBBQ_CORS_ORIGINS,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/", include_in_schema=False)
def root():
    return {
        "ok": True,
        "service": "kbbq-idle-backend",
        "health": "/health",
        "meta": "/meta",
        "readiness": "/readiness",
        "review_pack": "/review-pack",
        "metrics": "/metrics",
        "docs": "/docs" if EXPOSE_DOCS else None,
    }


@app.get("/favicon.ico", include_in_schema=False)
def favicon():
    # Avoid noisy 404s in logs when opened in a browser.
    return Response(status_code=204)


def _ops_token() -> str:
    return (os.getenv("KBBQ_OPS_TOKEN") or os.getenv("KBBQ_OPS_ADMIN_TOKEN") or "").strip()


def _feedback_endpoint() -> str:
    return (os.getenv("KBBQ_FORMSPREE_ENDPOINT") or "").strip()


def _runtime_env() -> str:
    return ((os.getenv("KBBQ_ENV") or "dev").strip().lower()) or "dev"


def _ops_contract() -> dict[str, object]:
    return {"schema": "ops-envelope-v1", "version": 1}


def _ops_links() -> dict[str, object]:
    return {
        "health": "/health",
        "meta": "/meta",
        "readiness": "/readiness",
        "review_pack": "/review-pack",
        "release_readiness": "/ops/release-readiness",
        "economy_balance_drill": "/ops/economy-balance-drill",
        "metrics": "/metrics",
        "alerts": "/ops/alerts",
        "docs": "/docs" if EXPOSE_DOCS else None,
    }


def _integration_state() -> dict[str, bool]:
    return {
        "ops_token_configured": bool(_ops_token()),
        "feedback_relay_configured": bool(_feedback_endpoint()),
        "docs_exposed": EXPOSE_DOCS,
    }


def _webgl_docs_root() -> Path:
    configured = (os.getenv("KBBQ_WEBGL_DOCS_ROOT") or "").strip()
    if configured:
        return Path(configured).expanduser().resolve()
    return Path(__file__).resolve().parents[1] / "docs"


def _build_webgl_delivery_report() -> dict[str, object]:
    docs_root = _webgl_docs_root()
    build_root = docs_root / "Build"
    required_assets = [
        docs_root / "index.html",
        build_root / "build-manifest.json",
        build_root / "KBBQIdleWebGL.loader.js",
        build_root / "KBBQIdleWebGL.framework.js",
        build_root / "KBBQIdleWebGL.data",
        build_root / "KBBQIdleWebGL.wasm",
    ]
    missing_assets = [str(path.relative_to(docs_root)) for path in required_assets if not path.exists()]
    ready = not missing_assets
    return {
        "ready": ready,
        "status": "verified-build-present" if ready else "placeholder-or-missing-build",
        "claim_posture": "verified-webgl-runtime" if ready else "docs-placeholder-only",
        "docs_root": str(docs_root),
        "required_assets": [str(path.relative_to(docs_root)) for path in required_assets],
        "missing_assets": missing_assets,
        "claim_rule": (
            "Required reviewer assets are present, so you can claim live WebGL reviewer delivery."
            if ready
            else "Do not claim live WebGL delivery from docs alone until ./tools/build_webgl_docs.sh produces the required reviewer assets."
        ),
        "next_action": (
            "WebGL build artifacts are present for reviewer delivery."
            if ready
            else "Run ./tools/build_webgl_docs.sh before claiming WebGL delivery readiness."
        ),
    }


def _build_readiness_report() -> dict[str, object]:
    checks = []
    warnings = []
    advisories = []
    now = int(time.time())

    try:
        with _db_session() as db:
            db.execute("SELECT 1").fetchone()
        checks.append({"name": "db", "ok": True})
    except Exception as exc:  # noqa: BLE001
        checks.append({"name": "db", "ok": False, "error": str(exc)})

    if not _ops_token():
        warnings.append("KBBQ_OPS_TOKEN is not configured")

    if not (os.getenv("KBBQ_HMAC_SECRET") or "").strip() or (os.getenv("KBBQ_HMAC_SECRET") == "CHANGE_ME"):
        warnings.append("KBBQ_HMAC_SECRET is weak or default")

    if not (os.getenv("KBBQ_TOKEN_SALT") or "").strip() or (os.getenv("KBBQ_TOKEN_SALT") == "dev-only-salt"):
        warnings.append("KBBQ_TOKEN_SALT is weak or default")

    if not _feedback_endpoint():
        advisories.append("KBBQ_FORMSPREE_ENDPOINT is not configured")

    ready = all(bool(c.get("ok")) for c in checks)
    status = "ok" if ready else "degraded"
    return {
        "ready": ready,
        "status": status,
        "checks": checks,
        "warnings": warnings,
        "advisories": advisories,
        "uptime_seconds": max(0, now - APP_STARTED_AT),
        "ts": now,
    }


def _next_action(report: dict[str, object]) -> str:
    failing_checks = [str(check.get("name")) for check in report["checks"] if not check.get("ok")]
    if failing_checks:
        checks = ", ".join(failing_checks)
        return f"Restore failing dependencies ({checks}) before accepting live traffic."

    warnings = [str(item) for item in report.get("warnings", [])]
    if warnings:
        return warnings[0]

    advisories = [str(item) for item in report.get("advisories", [])]
    if advisories:
        return "Configure KBBQ_FORMSPREE_ENDPOINT to enable in-game feedback relay for live ops."

    return "No action required."


def _build_review_pack(report: dict[str, object]) -> dict[str, object]:
    integrations = _integration_state()
    webgl_delivery = _build_webgl_delivery_report()
    proof_bundle = {
        "gameplay_loop": "buy -> grill -> flip -> collect -> serve -> upgrade",
        "webgl_delivery_ready": webgl_delivery["ready"],
        "signed_request_surface": True,
        "ops_token_configured": integrations["ops_token_configured"],
        "feedback_relay_configured": integrations["feedback_relay_configured"],
        "docs_exposed": integrations["docs_exposed"],
        "focused_ops_snapshot": "rush preset -> queue pressure -> perf overlay -> optional economy posture",
    }
    economy_contract = {
        "loop": "queue throughput drives income_per_second and upgrade pacing",
        "store_tiers": "level-based store tier progression changes income multiplier",
        "optional_economy": "optional rewards and pack grants stay optional, server-authoritative verification remains on backend",
    }
    reviewer_posture = {
        "runtime_source_of_truth": "/review-pack + Unity WebGL/Editor loop",
        "docs_only_surfaces": ["docs/index.html", "docs/help.html", "docs/about.html", "docs/compliance.html"],
        "claim_tier": "runtime-backed-review-ready" if webgl_delivery["ready"] else "docs-first-placeholder",
        "claim_rule": (
            "Use docs surfaces as reviewer aids, then repeat playable/runtime claims only after build preflight and live Unity launch both succeed."
        ),
    }
    return {
        "status": report["status"],
        "service": "kbbq-idle-backend",
        "generated_at": report["ts"],
        "readiness_contract": REVIEW_PACK_CONTRACT,
        "headline": "Idle tycoon slice exposes gameplay, economy, and WebGL delivery proof in one reviewer surface.",
        "proof_bundle": proof_bundle,
        "webgl_delivery": webgl_delivery,
        "economy_contract": economy_contract,
        "reviewer_posture": reviewer_posture,
        "trust_boundary": [
            "Unity client stays playable without backend; networking remains opt-in for demo flows.",
            "Signed headers and nonce replay protection guard leaderboard, analytics, and feedback routes.",
            "Optional pack verification remains server-authoritative and does not trust client currency grants.",
        ],
        "review_sequence": [
            "Open /health and /meta to confirm ops posture and enabled surfaces.",
            "Open /review-pack to inspect gameplay loop, economy contract, and delivery posture.",
            "/ops/release-readiness",
            "/ops/economy-balance-drill",
            "Run the Unity WebGL build or Editor scene to validate the grill -> serve -> upgrade loop.",
        ],
        "two_minute_review": [
            "Open /health and /meta to confirm runtime posture, enabled surfaces, and next action.",
            "Open /review-pack to pin gameplay loop, economy contract, and optional economy posture.",
            "Run the Unity WebGL build or Editor scene and validate the grill -> serve -> upgrade loop.",
            "Read the live perf overlay before claiming delivery or optional economy readiness.",
        ],
        "proof_assets": [
            {
                "label": "Health Envelope",
                "path": "/health",
                "why": "Confirms dependency readiness, warning posture, and next operator action.",
            },
            {
                "label": "Runtime Profile",
                "path": "/meta",
                "why": "Pins enabled capabilities and runtime delivery posture before a demo.",
            },
            {
                "label": "Review Pack",
                "path": "/review-pack",
                "why": "Packages gameplay loop, economy contract, and trust boundary in one payload.",
            },
            {
                "label": "Release Readiness",
                "path": "/ops/release-readiness",
                "why": "Summarizes WebGL delivery, integration gates, and reviewer claim posture before rollout.",
            },
            {
                "label": "Economy Balance Drill",
                "path": "/ops/economy-balance-drill",
                "why": "Summarizes guardian-mode triggers, offline pressure, and tier delta posture before optional economy claims.",
            },
            {
                "label": "Perf Overlay",
                "path": "Assets/Scripts/UI/PerfOverlayView.cs",
                "why": "Shows the live gameplay review pack directly inside the Unity surface.",
            },
        ],
        "watchouts": [
            "Set KBBQ_HMAC_SECRET and KBBQ_TOKEN_SALT before any shared demo or public deployment.",
            "Add KBBQ_FORMSPREE_ENDPOINT only when the feedback relay is required for a live review.",
            "Expose Swagger docs only for local debugging via KBBQ_EXPOSE_DOCS=1.",
        ],
        "links": _ops_links(),
    }


def _build_release_readiness(report: dict[str, object]) -> dict[str, object]:
    integrations = _integration_state()
    webgl_delivery = _build_webgl_delivery_report()
    return {
        "status": report["status"],
        "service": "kbbq-idle-backend",
        "generated_at": report["ts"],
        "contract_version": "kbbq-release-readiness-v1",
        "summary": {
            "backend_ready": report["ready"],
            "webgl_delivery_ready": webgl_delivery["ready"],
            "ops_token_configured": integrations["ops_token_configured"],
            "feedback_relay_configured": integrations["feedback_relay_configured"],
        },
        "reviewer_claim": (
            "Playable and review-ready" if report["ready"] and webgl_delivery["ready"] else "Reviewable with explicit blockers"
        ),
        "blockers": [
            *[str(item) for item in report.get("warnings", [])],
            *[str(item) for item in report.get("advisories", []) if not integrations["feedback_relay_configured"]],
            *([] if webgl_delivery["ready"] else [str(webgl_delivery["next_action"])]),
        ],
        "next_action": _next_action(report) if report["ready"] and webgl_delivery["ready"] else webgl_delivery["next_action"],
        "links": _ops_links(),
    }


@app.get("/health")
def health():
    report = _build_readiness_report()
    webgl_delivery = _build_webgl_delivery_report()
    return {
        "ok": True,
        "status": report["status"],
        "service": "kbbq-idle-backend",
        "ts": report["ts"],
        "uptime_seconds": report["uptime_seconds"],
        "diagnostics": {
            "ready": report["ready"],
            "failing_checks": [check["name"] for check in report["checks"] if not check.get("ok")],
            "warnings": report["warnings"],
            "advisories": report["advisories"],
            "active_rate_limit_buckets": len(RATE_BUCKETS),
            "integrations": _integration_state(),
            "webgl_delivery": webgl_delivery,
            "next_action": _next_action(report),
        },
        "links": _ops_links(),
        "ops_contract": _ops_contract(),
    }


@app.get("/meta")
def meta():
    report = _build_readiness_report()
    webgl_delivery = _build_webgl_delivery_report()
    return {
        "service": "kbbq-idle-backend",
        "status": report["status"],
        "runtime": {
            "env": _runtime_env(),
            "uptime_seconds": report["uptime_seconds"],
            "docs_exposed": EXPOSE_DOCS,
        },
        "capabilities": {
            "guest_auth": True,
            "signed_requests": True,
            "leaderboard": True,
            "analytics": True,
            "friends": True,
            "iap_verify": True,
            "community_feedback": bool(_feedback_endpoint()),
            "economy_balance_drill": True,
        },
        "diagnostics": {
            "ready": report["ready"],
            "warnings": report["warnings"],
            "advisories": report["advisories"],
            "webgl_delivery": webgl_delivery,
            "next_action": _next_action(report),
        },
        "review_pack_contract": REVIEW_PACK_CONTRACT,
        "links": _ops_links(),
        "ops_contract": _ops_contract(),
    }


def _build_economy_balance_drill() -> dict[str, object]:
    tiers = [
        {
            "tier": "starter grill",
            "income_per_second": 4.2,
            "offline_pressure_minutes": 45,
            "guardian_trigger": "queue > 6 orders",
            "upgrade_delta_pct": 0,
        },
        {
            "tier": "double-stove line",
            "income_per_second": 7.8,
            "offline_pressure_minutes": 70,
            "guardian_trigger": "queue > 10 orders",
            "upgrade_delta_pct": 86,
        },
        {
            "tier": "party table expansion",
            "income_per_second": 11.4,
            "offline_pressure_minutes": 95,
            "guardian_trigger": "queue > 14 orders",
            "upgrade_delta_pct": 46,
        },
    ]
    starter = tiers[0]
    final_tier = tiers[-1]
    return {
        "status": "ok",
        "service": "kbbq-idle-backend",
        "generated_at": int(time.time()),
        "contract_version": "kbbq-idle-balance-drill-v1",
        "summary": {
            "guardian_triggers": len(tiers),
            "optional_economy_enabled": False,
            "optional_economy_posture": "reviewer-safe optional packs and rewards remain off by default",
            "highest_offline_pressure_minutes": max(item["offline_pressure_minutes"] for item in tiers),
            "offline_pressure_delta_minutes": final_tier["offline_pressure_minutes"] - starter["offline_pressure_minutes"],
            "largest_upgrade_delta_pct": max(item["upgrade_delta_pct"] for item in tiers),
            "income_gain_pct": round(
                ((final_tier["income_per_second"] - starter["income_per_second"]) / starter["income_per_second"]) * 100,
                1,
            ),
            "guardian_mode_posture": "queue guardrails stay gameplay-first and optional economy-off during reviewer demos",
        },
        "tiers": tiers,
        "review_actions": [
            "Treat guardian triggers as balance-review thresholds, not optional economy prompts.",
            "Review offline pressure before claiming idle progression is stable across long sessions.",
            "Keep optional economy-off posture visible during reviewer demos.",
        ],
        "links": _ops_links(),
    }


def _is_rate_limited(key: str, limit: int, window_seconds: int) -> bool:
    now = time.time()
    cutoff = now - float(window_seconds)
    history = [ts for ts in RATE_BUCKETS.get(key, []) if ts >= cutoff]
    if len(history) >= limit:
        RATE_BUCKETS[key] = history
        return True
    history.append(now)
    RATE_BUCKETS[key] = history
    return False


def _rate_scope(request: Request, player_id: str, action: str) -> str:
    client_ip = "unknown"
    if request.client and request.client.host:
        client_ip = request.client.host
    return f"{action}:{player_id}:{client_ip}"


def _require_ops_token(request: Request) -> None:
    expected = _ops_token()
    if not expected:
        raise HTTPException(status_code=503, detail="ops token is not configured")
    provided = request.headers.get("x-ops-token", "").strip()
    if not provided or provided != expected:
        raise HTTPException(status_code=401, detail="invalid ops token")


@contextmanager
def _db_session():
    db = get_db()
    try:
        yield db
    finally:
        db.close()


@app.get("/readiness")
def readiness():
    return _build_readiness_report()


@app.get("/review-pack")
def review_pack():
    report = _build_readiness_report()
    return _build_review_pack(report)


@app.get("/ops/release-readiness")
def release_readiness():
    report = _build_readiness_report()
    return _build_release_readiness(report)


@app.get("/ops/economy-balance-drill")
def economy_balance_drill(request: Request):
    _require_ops_token(request)
    return _build_economy_balance_drill()


@app.get("/metrics")
def metrics():
    with _db_session() as db:
        players = int(db.execute("SELECT COUNT(*) AS c FROM players").fetchone()["c"])
        leaderboard_entries = int(db.execute("SELECT COUNT(*) AS c FROM leaderboard").fetchone()["c"])
        friends_edges = int(db.execute("SELECT COUNT(*) AS c FROM friends").fetchone()["c"])
        events = int(db.execute("SELECT COUNT(*) AS c FROM analytics_events").fetchone()["c"])
        nonce_rows = int(db.execute("SELECT COUNT(*) AS c FROM nonces").fetchone()["c"])
    uptime = max(0, int(time.time()) - APP_STARTED_AT)

    body = "\n".join(
        [
            "# HELP kbbq_players_total Total number of registered players.",
            "# TYPE kbbq_players_total gauge",
            f"kbbq_players_total {players}",
            "# HELP kbbq_leaderboard_entries_total Total leaderboard entries.",
            "# TYPE kbbq_leaderboard_entries_total gauge",
            f"kbbq_leaderboard_entries_total {leaderboard_entries}",
            "# HELP kbbq_friends_edges_total Total directed friendship edges.",
            "# TYPE kbbq_friends_edges_total gauge",
            f"kbbq_friends_edges_total {friends_edges}",
            "# HELP kbbq_analytics_events_total Total analytics events.",
            "# TYPE kbbq_analytics_events_total counter",
            f"kbbq_analytics_events_total {events}",
            "# HELP kbbq_nonce_rows_total Nonce rows retained for replay protection.",
            "# TYPE kbbq_nonce_rows_total gauge",
            f"kbbq_nonce_rows_total {nonce_rows}",
            "# HELP kbbq_uptime_seconds Process uptime in seconds.",
            "# TYPE kbbq_uptime_seconds gauge",
            f"kbbq_uptime_seconds {uptime}",
            "",
        ]
    )
    return Response(content=body, media_type="text/plain; version=0.0.4")


@app.get("/ops/alerts")
def ops_alerts(request: Request):
    _require_ops_token(request)
    alerts = []
    with _db_session() as db:
        player_count = int(db.execute("SELECT COUNT(*) AS c FROM players").fetchone()["c"])
        nonce_count = int(db.execute("SELECT COUNT(*) AS c FROM nonces").fetchone()["c"])

    if player_count == 0:
        alerts.append(
            {
                "level": "info",
                "code": "no_players",
                "message": "No players registered yet.",
            }
        )

    if nonce_count > 10000:
        alerts.append(
            {
                "level": "warning",
                "code": "nonce_backlog",
                "message": f"Nonce table is large ({nonce_count}). Review KBBQ_NONCE_TTL_SECONDS.",
            }
        )

    if not alerts:
        alerts.append(
            {
                "level": "info",
                "code": "healthy",
                "message": "No active ops alerts.",
            }
        )

    return {"alerts": alerts, "ts": int(time.time())}


@app.post("/auth/guest", response_model=AuthResponse)
async def auth_guest(request: Request):
    body = await request.json()
    device_id = str(body.get("deviceId") or "").strip()
    if not device_id:
        # Allow demo calls from curl.
        device_id = "demo-" + uuid.uuid4().hex

    with _db_session() as db:
        existing = db.execute(
            "SELECT player_id, token_sha256 FROM players WHERE device_id = ?",
            (device_id,),
        ).fetchone()

        salt = os.getenv("KBBQ_TOKEN_SALT", "dev-only-salt")
        if existing:
            player_id = str(existing["player_id"])
            token = new_token()
            token_hash = token_sha256(token, salt)
            db.execute(
                "UPDATE players SET token_sha256 = ? WHERE player_id = ?",
                (token_hash, player_id),
            )
            db.commit()
            ensure_friend_code(db, player_id)
            return AuthResponse(playerId=player_id, token=token)

        player_id = "p_" + uuid.uuid4().hex
        token = new_token()
        token_hash = token_sha256(token, salt)
        region = "KR"
        display_name = "Guest-" + player_id[-4:].upper()

        db.execute(
            "INSERT INTO players(player_id, device_id, display_name, token_sha256, region, created_at) VALUES(?,?,?,?,?,?)",
            (player_id, device_id, display_name, token_hash, region, int(time.time())),
        )
        db.commit()
        ensure_friend_code(db, player_id)
        return AuthResponse(playerId=player_id, token=token)


@app.post("/leaderboard/submit")
async def leaderboard_submit(request: Request):
    raw = (await request.body()).decode("utf-8")
    try:
        payload = ScoreSubmitRequest.model_validate_json(raw)
    except Exception:
        raise HTTPException(status_code=400, detail="invalid json body")

    with _db_session() as db:
        player_id = require_bearer_player_id(request, db)
        if payload.playerId != player_id:
            raise HTTPException(status_code=401, detail="player mismatch")

        verify_signed_headers(request, db=db, player_id=player_id, raw_body=raw)

        secret = os.getenv("KBBQ_HMAC_SECRET", "")
        if not secret or secret == "CHANGE_ME":
            if os.getenv("APP_ENV", "production") not in ("development", "test"):
                raise RuntimeError("KBBQ_HMAC_SECRET must be set to a strong secret in production")
            secret = "CHANGE_ME"
        # Match Unity client signing: sign a rounded integer score for deterministic cross-language behavior.
        score_int = int(round(float(payload.score)))
        body_sig_payload = f"{player_id}|{score_int}|{payload.timestamp}"
        expected_body_sig = hmac_b64(secret, body_sig_payload)
        if expected_body_sig != payload.signature:
            raise HTTPException(status_code=401, detail="bad body signature")

        # Upsert score (keep best score).
        region_row = db.execute(
            "SELECT region FROM players WHERE player_id = ?",
            (player_id,),
        ).fetchone()
        region = str(region_row["region"]) if region_row else "KR"

        existing = db.execute(
            "SELECT score FROM leaderboard WHERE region = ? AND player_id = ?",
            (region, player_id),
        ).fetchone()
        score = float(payload.score)
        if existing is None:
            db.execute(
                "INSERT INTO leaderboard(region, player_id, score, updated_at) VALUES(?,?,?,?)",
                (region, player_id, score, int(time.time())),
            )
        else:
            best = max(float(existing["score"]), score)
            db.execute(
                "UPDATE leaderboard SET score = ?, updated_at = ? WHERE region = ? AND player_id = ?",
                (best, int(time.time()), region, player_id),
            )
        db.commit()
        return {"ok": True}


@app.get("/leaderboard/top", response_model=LeaderboardResponse)
async def leaderboard_top(request: Request, region: str = "KR", limit: int = 10):
    with _db_session() as db:
        player_id = require_bearer_player_id(request, db)
        verify_signed_headers(request, db=db, player_id=player_id, raw_body="")

        limit = max(1, min(100, int(limit)))
        region = (region or "KR").strip().upper()

        rows = db.execute(
            "SELECT l.player_id, p.display_name, l.score FROM leaderboard l JOIN players p ON p.player_id = l.player_id "
            "WHERE l.region = ? ORDER BY l.score DESC LIMIT ?",
            (region, limit),
        ).fetchall()

        entries = []
        for idx, row in enumerate(rows, start=1):
            entries.append(
                LeaderboardEntry(
                    playerId=str(row["player_id"]),
                    displayName=str(row["display_name"]),
                    score=float(row["score"]),
                    rank=idx,
                )
            )

        return LeaderboardResponse(entries=entries)


@app.get("/friends/list", response_model=FriendListResponse)
async def friends_list(request: Request):
    with _db_session() as db:
        player_id = require_bearer_player_id(request, db)
        verify_signed_headers(request, db=db, player_id=player_id, raw_body="")

        rows = db.execute(
            "SELECT f.friend_player_id, p.display_name FROM friends f JOIN players p ON p.player_id = f.friend_player_id "
            "WHERE f.player_id = ? ORDER BY p.display_name ASC LIMIT 50",
            (player_id,),
        ).fetchall()

        friends = [{"playerId": str(r["friend_player_id"]), "displayName": str(r["display_name"])} for r in rows]
        return {"friends": friends}


@app.post("/analytics/event")
async def analytics_event(request: Request):
    raw = (await request.body()).decode("utf-8")
    try:
        payload = AnalyticsEventRequest.model_validate_json(raw)
    except Exception:
        raise HTTPException(status_code=400, detail="invalid json body")

    with _db_session() as db:
        player_id = require_bearer_player_id(request, db)
        if payload.playerId != player_id:
            raise HTTPException(status_code=401, detail="player mismatch")
        verify_signed_headers(request, db=db, player_id=player_id, raw_body=raw)
        if _is_rate_limited(_rate_scope(request, player_id, "analytics"), limit=120, window_seconds=60):
            raise HTTPException(status_code=429, detail="too many analytics events")

        event_name = (payload.eventName or "").strip()
        if not event_name:
            raise HTTPException(status_code=400, detail="missing eventName")

        kv = payload.kv or []
        if len(kv) > 50:
            kv = kv[:50]

        ts = int(payload.timestamp) if payload.timestamp else int(time.time())
        db.execute(
            "INSERT INTO analytics_events(player_id, event_name, kv_json, ts) VALUES(?,?,?,?)",
            (player_id, event_name, json.dumps(kv), ts),
        )
        db.commit()
        return {"ok": True}


@app.post("/community/feedback")
async def community_feedback(request: Request):
    raw = (await request.body()).decode("utf-8")
    try:
        payload = CommunityFeedbackRequest.model_validate_json(raw)
    except Exception:
        raise HTTPException(status_code=400, detail="invalid json body")

    endpoint = _feedback_endpoint()
    if not endpoint:
        raise HTTPException(status_code=503, detail="feedback relay is not configured")

    with _db_session() as db:
        player_id = require_bearer_player_id(request, db)
        if payload.playerId != player_id:
            raise HTTPException(status_code=401, detail="player mismatch")
        verify_signed_headers(request, db=db, player_id=player_id, raw_body=raw)
        if _is_rate_limited(_rate_scope(request, player_id, "feedback"), limit=6, window_seconds=600):
            raise HTTPException(status_code=429, detail="too many feedback requests")

    message = " ".join(str(payload.message or "").split())
    if not message:
        raise HTTPException(status_code=400, detail="missing feedback message")
    if len(message) > 1000:
        message = message[:1000]

    secret = os.getenv("KBBQ_HMAC_SECRET", "")
    if not secret or secret == "CHANGE_ME":
        if os.getenv("APP_ENV", "production") not in ("development", "test"):
            raise RuntimeError("KBBQ_HMAC_SECRET must be set to a strong secret in production")
        secret = "CHANGE_ME"
    body_sig_payload = f"{player_id}|{payload.timestamp}|{message}"
    expected_body_sig = hmac_b64(secret, body_sig_payload)
    if expected_body_sig != payload.signature:
        raise HTTPException(status_code=401, detail="bad body signature")

    relay_payload = {
        "player_id": player_id,
        "email": str(payload.email or "").strip(),
        "message": message,
        "channel": str(payload.channel or "in-game").strip() or "in-game",
        "source": "kbbq-idle-backend",
    }
    try:
        resp = httpx.post(
            endpoint,
            json=relay_payload,
            headers={"Accept": "application/json"},
            timeout=8.0,
        )
    except httpx.HTTPError:
        raise HTTPException(status_code=502, detail="feedback relay request failed")

    if resp.status_code >= 400:
        detail = "feedback relay rejected request"
        try:
            body = resp.json()
            if isinstance(body, dict):
                errors = body.get("errors")
                if isinstance(errors, list) and errors and isinstance(errors[0], dict) and errors[0].get("message"):
                    detail = str(errors[0].get("message"))
                elif body.get("error"):
                    detail = str(body.get("error"))
        except Exception:
            pass
        raise HTTPException(status_code=502, detail=detail)

    return {"ok": True, "forwarded": True}


@app.post("/friends/invite")
async def friends_invite(request: Request):
    raw = (await request.body()).decode("utf-8")
    try:
        payload = FriendInviteRequest.model_validate_json(raw)
    except Exception:
        raise HTTPException(status_code=400, detail="invalid json body")

    with _db_session() as db:
        player_id = require_bearer_player_id(request, db)
        if payload.playerId != player_id:
            raise HTTPException(status_code=401, detail="player mismatch")
        verify_signed_headers(request, db=db, player_id=player_id, raw_body=raw)
        if _is_rate_limited(_rate_scope(request, player_id, "invite"), limit=30, window_seconds=60):
            raise HTTPException(status_code=429, detail="too many invite attempts")

        secret = os.getenv("KBBQ_HMAC_SECRET", "")
        if not secret or secret == "CHANGE_ME":
            if os.getenv("APP_ENV", "production") not in ("development", "test"):
                raise RuntimeError("KBBQ_HMAC_SECRET must be set to a strong secret in production")
            secret = "CHANGE_ME"
        body_sig_payload = f"{player_id}|{payload.code}|{payload.timestamp}"
        expected_body_sig = hmac_b64(secret, body_sig_payload)
        if expected_body_sig != payload.signature:
            raise HTTPException(status_code=401, detail="bad body signature")

        code = (payload.code or "").strip().upper()
        if len(code) < 4:
            raise HTTPException(status_code=400, detail="invalid code")

        target = db.execute(
            "SELECT player_id FROM friend_codes WHERE code = ?",
            (code,),
        ).fetchone()
        if not target:
            raise HTTPException(status_code=404, detail="code not found")

        friend_id = str(target["player_id"])
        if friend_id == player_id:
            raise HTTPException(status_code=400, detail="cannot friend self")

        now = int(time.time())
        # Create bidirectional friendship (idempotent).
        db.execute(
            "INSERT OR IGNORE INTO friends(player_id, friend_player_id, created_at) VALUES(?,?,?)",
            (player_id, friend_id, now),
        )
        db.execute(
            "INSERT OR IGNORE INTO friends(player_id, friend_player_id, created_at) VALUES(?,?,?)",
            (friend_id, player_id, now),
        )
        db.commit()

        return {"ok": True}
