import base64
import hashlib
import hmac
import os
import secrets
import sqlite3
import time
from typing import Optional

from fastapi import HTTPException, Request


def sha256_hex(data: str) -> str:
    return hashlib.sha256(data.encode("utf-8")).hexdigest()


def token_sha256(token: str, salt: str) -> str:
    # Salted so tokens aren't directly reversible from the DB.
    return sha256_hex(f"{salt}:{token}")


def new_token() -> str:
    # URL-safe enough for headers.
    return secrets.token_urlsafe(32)


def hmac_b64(secret: str, payload: str) -> str:
    key = secret.encode("utf-8")
    msg = payload.encode("utf-8")
    digest = hmac.new(key, msg, hashlib.sha256).digest()
    return base64.b64encode(digest).decode("utf-8")


def require_bearer_player_id(request: Request, db) -> str:
    auth = request.headers.get("authorization", "")
    if not auth.lower().startswith("bearer "):
        raise HTTPException(status_code=401, detail="missing bearer token")
    token = auth.split(" ", 1)[1].strip()
    if not token:
        raise HTTPException(status_code=401, detail="empty bearer token")

    salt = os.getenv("KBBQ_TOKEN_SALT", "dev-only-salt")
    token_hash = token_sha256(token, salt)
    row = db.execute(
        "SELECT player_id FROM players WHERE token_sha256 = ?",
        (token_hash,),
    ).fetchone()
    if not row:
        raise HTTPException(status_code=401, detail="invalid token")
    return str(row["player_id"])


def verify_signed_headers(
    request: Request,
    *,
    db,
    player_id: str,
    raw_body: str,
) -> None:
    secret = os.getenv("KBBQ_HMAC_SECRET", "CHANGE_ME")
    if not secret:
        raise HTTPException(status_code=500, detail="server misconfigured: missing HMAC secret")

    nonce = request.headers.get("x-nonce", "").strip()
    ts_raw = request.headers.get("x-timestamp", "").strip()
    sig = request.headers.get("x-signature", "").strip()
    if not nonce or not ts_raw or not sig:
        raise HTTPException(status_code=401, detail="missing signed headers")

    try:
        ts = int(ts_raw)
    except ValueError:
        raise HTTPException(status_code=401, detail="invalid timestamp")

    now = int(time.time())
    skew = int(os.getenv("KBBQ_MAX_CLOCK_SKEW_SECONDS", "300"))
    if abs(now - ts) > max(30, skew):
        raise HTTPException(status_code=401, detail="timestamp out of range")

    payload = f"{player_id}|{nonce}|{ts}|{raw_body or ''}"
    expected = hmac_b64(secret, payload)
    if not hmac.compare_digest(expected, sig):
        raise HTTPException(status_code=401, detail="bad signature")

    # Replay protection: store nonce for a short TTL.
    # Insert only after successful signature verification to avoid blocking legit requests with the same nonce.
    try:
        db.execute(
            "INSERT INTO nonces(player_id, nonce, ts) VALUES(?,?,?)",
            (player_id, nonce, ts),
        )
        db.commit()
    except sqlite3.IntegrityError:
        raise HTTPException(status_code=401, detail="replay detected (nonce reused)")


def ensure_friend_code(db, player_id: str) -> str:
    row = db.execute(
        "SELECT code FROM friend_codes WHERE player_id = ?",
        (player_id,),
    ).fetchone()
    if row:
        return str(row["code"])

    # Human-friendly code for demo purposes (avoid ambiguous chars).
    alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"
    code = "".join(secrets.choice(alphabet) for _ in range(6))

    # Retry on rare collisions.
    for _ in range(5):
        try:
            db.execute(
                "INSERT INTO friend_codes(player_id, code) VALUES(?,?)",
                (player_id, code),
            )
            db.commit()
            return code
        except Exception:
            code = "".join(secrets.choice(alphabet) for _ in range(6))

    raise HTTPException(status_code=500, detail="failed to allocate friend code")

