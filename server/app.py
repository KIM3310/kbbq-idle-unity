import json
import os
import time
import uuid
from contextlib import contextmanager

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
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/", include_in_schema=False)
def root():
    return {"ok": True, "service": "kbbq-idle-backend", "health": "/health", "docs": "/docs" if EXPOSE_DOCS else None}


@app.get("/favicon.ico", include_in_schema=False)
def favicon():
    # Avoid noisy 404s in logs when opened in a browser.
    return Response(status_code=204)


@app.get("/health")
def health():
    return {"ok": True, "ts": int(time.time())}


def _ops_token() -> str:
    return (os.getenv("KBBQ_OPS_TOKEN") or os.getenv("KBBQ_OPS_ADMIN_TOKEN") or "").strip()


def _feedback_endpoint() -> str:
    return (os.getenv("KBBQ_FORMSPREE_ENDPOINT") or "").strip()


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
    checks = []
    warnings = []
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

    ready = all(bool(c.get("ok")) for c in checks)
    return {
        "ready": ready,
        "checks": checks,
        "warnings": warnings,
        "uptime_seconds": max(0, now - APP_STARTED_AT),
        "ts": now,
    }


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

        secret = os.getenv("KBBQ_HMAC_SECRET", "CHANGE_ME")
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

    message = " ".join(str(payload.message or "").split())
    if not message:
        raise HTTPException(status_code=400, detail="missing feedback message")
    if len(message) > 1000:
        message = message[:1000]

    secret = os.getenv("KBBQ_HMAC_SECRET", "CHANGE_ME")
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

        secret = os.getenv("KBBQ_HMAC_SECRET", "CHANGE_ME")
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
