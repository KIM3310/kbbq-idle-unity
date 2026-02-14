import os
import sqlite3
import threading
import time
from dataclasses import dataclass


@dataclass(frozen=True)
class DbConfig:
    path: str


_lock = threading.Lock()


def _connect(path: str) -> sqlite3.Connection:
    conn = sqlite3.connect(path, check_same_thread=False)
    conn.row_factory = sqlite3.Row
    return conn


def get_db() -> sqlite3.Connection:
    path = os.getenv("KBBQ_DB_PATH", os.path.join(os.getcwd(), "kbbq.db"))
    os.makedirs(os.path.dirname(path), exist_ok=True) if os.path.dirname(path) else None
    conn = _connect(path)
    _ensure_schema(conn)
    return conn


def _ensure_schema(conn: sqlite3.Connection) -> None:
    # Keep this idempotent and small (single-file demo DB).
    with _lock:
        conn.execute(
            """
            CREATE TABLE IF NOT EXISTS players (
              player_id TEXT PRIMARY KEY,
              device_id TEXT UNIQUE,
              display_name TEXT NOT NULL,
              token_sha256 TEXT NOT NULL,
              region TEXT NOT NULL,
              created_at INTEGER NOT NULL
            );
            """
        )
        conn.execute(
            """
            CREATE TABLE IF NOT EXISTS nonces (
              player_id TEXT NOT NULL,
              nonce TEXT NOT NULL,
              ts INTEGER NOT NULL,
              PRIMARY KEY (player_id, nonce)
            );
            """
        )
        conn.execute(
            """
            CREATE TABLE IF NOT EXISTS leaderboard (
              region TEXT NOT NULL,
              player_id TEXT NOT NULL,
              score REAL NOT NULL,
              updated_at INTEGER NOT NULL,
              PRIMARY KEY (region, player_id)
            );
            """
        )
        conn.execute(
            """
            CREATE TABLE IF NOT EXISTS friend_codes (
              player_id TEXT PRIMARY KEY,
              code TEXT UNIQUE NOT NULL
            );
            """
        )
        conn.execute(
            """
            CREATE TABLE IF NOT EXISTS friends (
              player_id TEXT NOT NULL,
              friend_player_id TEXT NOT NULL,
              created_at INTEGER NOT NULL,
              PRIMARY KEY (player_id, friend_player_id)
            );
            """
        )
        conn.commit()

        # Opportunistic cleanup (nonce TTL)
        ttl = int(os.getenv("KBBQ_NONCE_TTL_SECONDS", "600"))
        cutoff = int(time.time()) - max(1, ttl)
        conn.execute("DELETE FROM nonces WHERE ts < ?", (cutoff,))
        conn.commit()

