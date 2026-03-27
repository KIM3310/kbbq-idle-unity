# KBBQ Idle Runbook (Local Demo)

This repo is a Unity project (2022.3 LTS). The main gameplay runs in a single scene:
`Assets/Scenes/Main.unity`.

## Prerequisites
- Unity `2022.3.62f3`

## Run Locally
1. Open the project in Unity Hub.
2. Open scene: `Assets/Scenes/Main.unity`.
3. Press Play.

## Demo Script (3 minutes)
1. Start the game and observe passive income increasing.
2. Buy an upgrade and confirm income/sec increases.
3. Use the boost button and confirm temporary multiplier.
4. Tap “Serve” a few times to see tips/combos and satisfaction impact.
5. Open missions/login UI and claim at least one reward.
6. Open monetization UI and verify reward/ad or pack purchase status feedback.
7. (Optional) Open the leaderboard panel:
   - With networking disabled (default), it uses mock data.
   - With networking enabled + backend running, it fetches live entries.

## Common Issues
- Scene missing bindings:
  - Run **KBBQ/Run Auto Setup** (Editor menu) to regenerate data assets and UI prefabs.
- Runtime watchdog retuning:
  - Run **KBBQ/Tune Runtime Diagnostics (Portfolio)** to recompute queue watchdog thresholds from stress profiles.
  - In Play mode, open Debug panel and click **Retune from Stress** for live retuning.
- Queue service guardian:
  - When runtime pressure stays high, guardian mode automatically throttles spawn and boosts service, then returns to the operator profile after recovery.
  - Guardian profiles:
    - `Light`: softer intervention for smoother pacing.
    - `Balanced`: recommended default.
    - `HardSafe`: aggressive protection for demo/low-end devices.
  - Apply profile globally from Editor menu:
    - `KBBQ > Service Guardian Profile > Apply Light/Balanced/HardSafe`
  - Auto-recommend and apply by objective:
    - `KBBQ > Service Guardian Profile > Recommend and Apply > Growth Priority`
    - `KBBQ > Service Guardian Profile > Recommend and Apply > Balanced Default`
    - `KBBQ > Service Guardian Profile > Recommend and Apply > Stability First`
  - Print recommendation report only (no apply):
    - `KBBQ > Service Guardian Profile > Report Recommendation (Growth/Balanced/Stability)`
  - Watch `Guardian` and `Rates` fields in the perf overlay to verify live behavior.
- Save data corruption fallback:
  - Save system keeps a validated backup slot and auto-recovers when primary data is corrupted.
- Networking warnings:
  - Networking is disabled by default in this repo. If you enabled it, set a real base URL and secret in `Assets/Data/Config/ApiConfig.asset`.

## CI/Batch Checks
Run all local checks (EditMode + PlayMode + data validation):
```bash
tools/ci_unity_checks.sh
```

Run the full portfolio quality gate (sim + server + Unity):
```bash
tools/portfolio_quality_gate.sh
```

Use a custom Unity binary (optional):
```bash
UNITY_BIN="/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity" tools/ci_unity_checks.sh
```

## Optional Backend (Leaderboard + Friends + Analytics)
Run with Docker:
```bash
KBBQ_HMAC_SECRET=CHANGE_ME docker compose up --build
```

Run with Python:
```bash
python3 -m venv .venv && ./.venv/bin/pip install -r server/requirements.txt && KBBQ_HMAC_SECRET=CHANGE_ME ./.venv/bin/uvicorn server.app:app --reload --port 8000
```

Enable Swagger UI locally (off by default):
```bash
KBBQ_EXPOSE_DOCS=1
```

Unity opt-in:
- Open `Assets/Data/Config/ApiConfig.asset`
- Set:
  - `enableNetwork=true`
  - `allowInEditor=true` (if running in the Editor)
  - `baseUrl=http://127.0.0.1:8000`
  - `hmacSecret=CHANGE_ME` (must match `KBBQ_HMAC_SECRET`)

Optional IAP verify path:
- Monetization purchase flow can call `POST /iap/verify` to confirm pack grants server-side.
- Default catalog (when not overridden): `starter=500`, `premium=4000`.

Ops endpoints:
- `GET /readiness`
- `GET /metrics`
- `GET /ops/alerts` (requires `X-Ops-Token`)

Ops scripts:
- `tools/deploy_backend.sh`
- `tools/check_backend_ops.sh`
- `tools/backup_kbbq_db.sh`
