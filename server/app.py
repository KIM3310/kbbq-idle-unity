import json
import os
import time
import uuid

from fastapi import FastAPI, HTTPException, Request, Response
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import RedirectResponse

from server.db import get_db
from server.models import (
    AnalyticsEventRequest,
    AuthResponse,
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


app = FastAPI(title="KBBQ Idle Backend", version="0.1")
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/", include_in_schema=False)
def root():
    # Portfolio/demo quality-of-life: opening the base URL should show something useful.
    return RedirectResponse(url="/docs")


@app.get("/favicon.ico", include_in_schema=False)
def favicon():
    # Avoid noisy 404s in logs when opened in a browser.
    return Response(status_code=204)


@app.get("/health")
def health():
    return {"ok": True, "ts": int(time.time())}


@app.post("/auth/guest", response_model=AuthResponse)
async def auth_guest(request: Request):
    body = await request.json()
    device_id = str(body.get("deviceId") or "").strip()
    if not device_id:
        # Allow demo calls from curl.
        device_id = "demo-" + uuid.uuid4().hex

    db = get_db()
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
    db = get_db()
    player_id = require_bearer_player_id(request, db)

    raw = (await request.body()).decode("utf-8")
    try:
        payload = ScoreSubmitRequest.model_validate_json(raw)
    except Exception:
        raise HTTPException(status_code=400, detail="invalid json body")

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
    db = get_db()
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
    db = get_db()
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
    db = get_db()
    player_id = require_bearer_player_id(request, db)

    raw = (await request.body()).decode("utf-8")
    try:
        payload = AnalyticsEventRequest.model_validate_json(raw)
    except Exception:
        raise HTTPException(status_code=400, detail="invalid json body")

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


@app.post("/friends/invite")
async def friends_invite(request: Request):
    db = get_db()
    player_id = require_bearer_player_id(request, db)

    raw = (await request.body()).decode("utf-8")
    try:
        payload = FriendInviteRequest.model_validate_json(raw)
    except Exception:
        raise HTTPException(status_code=400, detail="invalid json body")

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
