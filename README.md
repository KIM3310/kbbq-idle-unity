# KBBQ Idle — Unity 2022 Idle/Tycoon (Personal Project, 2022)

An idle/tycoon-style K-BBQ restaurant game built in Unity (2022.3 LTS). The gameplay centers on passive income, upgrades, queue management, unlock progression, and prestige for long-term growth.

License: MIT (see `LICENSE`). Third-party notices: `THIRD_PARTY_NOTICES.md`.

## Latest Update (February 17, 2026)
- Backend ops endpoints added: `/readiness`, `/metrics`, `/ops/alerts`
- IAP verification modes expanded: `mock`, `structured`, `store`
- Service docs/specs refreshed for portfolio + production handoff

## Docs
- English: `README.en.md`
- Korean: `README.ko.md`
- Deeper technical summary: `PROJECT_SUMMARY.en.md`, `PROJECT_SUMMARY.ko.md`
- Service-upgrade specs/logs: `docs/SPECKIT_SERVICE_100.ko.md`, `docs/COT_EXECUTION_LOG.ko.md`

## Quickstart
1. Open the project with Unity `2022.3.62f3`.
2. Load `Assets/Scenes/Main.unity`.
3. Press Play.

Optional:
- Run **KBBQ/Run Auto Setup** to regenerate ScriptableObject data assets and UI prefabs.

## Tech Notes
- Save integrity: versioned PlayerPrefs JSON (`KBBQ_IDLE_SAVE`) + SHA-256 checksum (`KBBQ_IDLE_SAVE_SHA256`) + defensive sanitize/clamp to handle corruption/tampering cases
- Economy: menu income x upgrades x store tier x boosts x tips/combos x prestige
- Sessions: offline earnings, daily login, daily missions
- Networking/monetization:
  - Network is opt-in and disabled by default (safe portfolio baseline)
  - Leaderboard/Friends/Analytics use signed requests against the optional backend
  - IAP purchase flow includes optional server verification (`/iap/verify`) before granting currency
  - Real store SDK receipt verification (Apple/Google) is intentionally left as an integration step

## Deterministic Simulator (Tests)
To keep the math reviewable outside Unity, `sim/` contains a small .NET project with unit tests for the economy/progression formulas.
Tested with .NET SDK `10.x`.

```bash
dotnet test sim/KbbqIdle.Sim.Tests/KbbqIdle.Sim.Tests.csproj
```

## Optional Backend (Leaderboard + Friends)
This repo includes a small FastAPI + SQLite backend in `server/` that matches the Unity network clients:
- `POST /auth/guest` (guest auth)
- `POST /leaderboard/submit`, `GET /leaderboard/top`
- `POST /analytics/event` (lightweight event ingestion)
- `POST /community/feedback` (Formspree relay for in-game feedback)
- `POST /friends/invite`, `GET /friends/list`
- `POST /iap/verify` (server-authoritative IAP grant + idempotency)
- `GET /readiness` (service readiness checks)
- `GET /metrics`, `GET /ops/alerts` (ops monitoring)

Run with Docker:
```bash
export KBBQ_HMAC_SECRET="CHANGE_ME"
docker compose up --build
```
Details: `server/README.md`.

Optional community relay:
- `KBBQ_FORMSPREE_ENDPOINT=https://formspree.io/f/...`

## Portfolio Quality Gate
Run the full local portfolio gate:

```bash
tools/portfolio_quality_gate.sh
```

This runs:
- deterministic simulator tests (`sim/`)
- backend tests (`server/tests`)
- Unity EditMode + PlayMode + data validator (`tools/ci_unity_checks.sh`)

Notes:
- If `.NET SDK` is missing, sim tests are skipped by default.
- Use `STRICT_PORTFOLIO_GATE=1 tools/portfolio_quality_gate.sh` to fail when any gate is unavailable.

Portfolio demo checklist: `PORTFOLIO_CHECKLIST.ko.md`

## Service Ops
- Production/staging env templates:
  - `server/.env.production.example`
  - `server/.env.staging.example`
- Backend deploy helper: `tools/deploy_backend.sh`
- Ops check helper: `tools/check_backend_ops.sh`
- DB backup helper: `tools/backup_kbbq_db.sh`

## Data Validation (Editor Utility)
In Unity, run:
- `KBBQ/Validate Data (Portfolio)`

This checks common issues (duplicate IDs, invalid tuning values, unsafe network defaults) and is also covered by EditMode tests.

## WebGL Demo (GitHub Pages)
This repo is prepared to host a WebGL build via GitHub Pages under `docs/`.

Build output:
- Unity Editor: `KBBQ/Build WebGL (docs)`
- CLI: `tools/build_webgl_docs.sh` (requires Unity installed)

After building, commit the generated `docs/` folder and push to `main`. The `pages` workflow will deploy it.

Cloudflare Pages deployment is also supported:
- Framework preset: `None`
- Root directory: `.`
- Build command: `(none)`
- Output directory: `docs`
- WebGL entry page: `docs/index.html` (loader UI with build name input + `Check Build Files`)
- Policy/ad crawl files: `docs/privacy.html`, `docs/terms.html`, `docs/contact.html`, `docs/compliance.html`, `docs/ads.txt`, `docs/robots.txt`, `docs/sitemap.xml`
- Cloudflare security headers template: `docs/_headers`

WebGL loader behavior:
- Uses `Build/<name>.json` first when available
- Falls back to auto-detecting `.unityweb`, `.br`, `.gz`, and plain build file variants

## Glossary (first-time readers)
- HMAC: Hash-based Message Authentication Code (request signing)
- SHA-256: Secure Hash Algorithm 256-bit (checksum / integrity)
