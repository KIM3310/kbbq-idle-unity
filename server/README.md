# KBBQ Idle Backend (Local Demo)

This is an optional backend to demonstrate:
- guest auth (token issuance),
- HMAC-signed request headers (replay-resistant),
- leaderboard submit/fetch,
- lightweight analytics event ingestion,
- simple friends list/invite flow,
- SQLite persistence.

It is intentionally small and self-contained so reviewers can run it quickly.

## Quickstart (Python)
```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r server/requirements.txt

export KBBQ_HMAC_SECRET="CHANGE_ME"
uvicorn server.app:app --reload --port 8000
```

Health check:
```bash
curl -s http://127.0.0.1:8000/health | jq .
```

Optional (enable Swagger UI for local debugging):
```bash
export KBBQ_EXPOSE_DOCS=1
```

## Quickstart (Docker)
```bash
export KBBQ_HMAC_SECRET="CHANGE_ME"
docker compose up --build
```

## Unity Config (opt-in)
Networking is **disabled by default** in the Unity project. To enable it locally:
1. Open `Assets/Data/Config/ApiConfig.asset`
2. Set:
   - `baseUrl`: `http://127.0.0.1:8000`
   - `enableNetwork`: `true`
   - `allowInEditor`: `true` (if running in the Editor)
   - `hmacSecret`: same value as `KBBQ_HMAC_SECRET`

## Security Notes
- Tokens are stored as SHA-256 hashes in SQLite.
- HMAC verification uses the *raw request body* (to match Unity's `JsonUtility` output).
- Signed headers are replay-protected via a nonce table with TTL.
- Leaderboard body signature signs a **rounded integer score** to avoid cross-language float string mismatches.
