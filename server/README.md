# KBBQ Idle Backend (Local Demo)

This is an optional backend to demonstrate:
- guest auth (token issuance),
- HMAC-signed request headers (replay-resistant),
- leaderboard submit/fetch,
- lightweight analytics event ingestion,
- community feedback relay (`/community/feedback`) to Formspree,
- simple friends list/invite flow,
- IAP verification (`/iap/verify`) with server-authoritative grants and tx idempotency,
- service readiness diagnostics (`/readiness`),
- ops/monitoring endpoints (`/metrics`, `/ops/alerts`),
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
- IAP verify does not trust client currency values; it uses server catalog values and enforces transaction id uniqueness.

## Env Flags (Production Hardening)
- `KBBQ_ENV=prod|staging|test|dev`
- `KBBQ_CORS_ORIGINS=https://your.domain` (comma-separated)
- `KBBQ_IAP_RECEIPT_MODE=mock|structured|store`
- `KBBQ_IAP_PRODUCT_IDS_JSON='{"starter":"com.yourgame.starter","premium":"com.yourgame.premium"}'`
- `KBBQ_APPLE_SHARED_SECRET=...`
- `KBBQ_GOOGLE_PACKAGE_NAME=com.yourgame.app`
- `KBBQ_GOOGLE_SERVICE_ACCOUNT_JSON='{"type":"service_account",...}'`
- `KBBQ_OPS_ADMIN_TOKEN=...`
- `KBBQ_FORMSPREE_ENDPOINT=https://formspree.io/f/...`

Production/staging templates:
- `server/.env.production.example`
- `server/.env.staging.example`

## Deployment/Ops Helpers
- Local deploy: `tools/deploy_backend.sh`
- Ops probe: `tools/check_backend_ops.sh`
- SQLite backup rotation: `tools/backup_kbbq_db.sh`
- GitHub workflows:
  - `.github/workflows/backend-deploy.yml`
  - `.github/workflows/backend-ops-monitor.yml`
