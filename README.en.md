# KBBQ Idle (Unity 2022)

## Overview
This is a Unity `2022.3.62f3` LTS project for an idle/tycoon K‑BBQ restaurant game. The project is built around a single scene at `Assets/Scenes/Main.unity` and focuses on passive income, upgrades, and long‑term progression.

License: MIT (see `LICENSE`). Third-party notices: `THIRD_PARTY_NOTICES.md`.

## Latest Update (February 17, 2026)
- Added backend ops endpoints: `/readiness`, `/metrics`, `/ops/alerts`
- Expanded IAP verification modes: `mock`, `structured`, `store`
- Refreshed docs/specs for portfolio and production handoff

## Gameplay Highlights
- Economy system combines menu income, upgrade multipliers, store tiers, boosts, tips, combos, and prestige.
- Customer queue with satisfaction + patience, plus optional manual “Serve” for tips and combos.
- Progression by total income unlocks menu items and store tiers.
- Prestige resets grant long‑term multipliers.
- Offline earnings, daily login rewards, and daily missions.
- Short tutorial flow (boost → upgrade → serve).

## Project Structure
- `GameManager` orchestrates systems and the main tick loop.
- `GameStateMachine` handles Boot/Tutorial/MainLoop/Pause/OfflineCalc.
- `SaveSystem` persists `SaveData` in PlayerPrefs (`KBBQ_IDLE_SAVE`).
- `UIController` binds missions, prestige, queue, upgrades, leaderboard, and monetization views.
- ScriptableObject data lives in `Assets/Data` (`GameDataCatalog`, `EconomyTuning`, `MonetizationConfig`, `ApiConfig`).

## Monetization & Networking
- Monetization rewards and IAP packs are configured with a local-safe default flow.
- Optional network clients use HMAC-signed headers. The base URL is a placeholder by default, so leaderboards use mock data unless networking is explicitly enabled.
  - When enabled + backend running, `LeaderboardView` submits the current score (best-effort) and fetches the live top list (fallbacks to mock data on failure).
  - IAP purchase path can verify through backend endpoint `POST /iap/verify` before currency is granted.
- Real store receipt verification (App Store / Play Store) is intentionally left as a production integration step.

## Quick Start
- Open with Unity `2022.3.62f3`.
- Load `Assets/Scenes/Main.unity` and press Play.
- Optional: run **KBBQ/Run Auto Setup** to regenerate data assets and UI prefabs.

## Portfolio Add-ons
- Optional backend (`server/`): guest auth + HMAC-signed leaderboard/friends + lightweight analytics ingestion + IAP verification endpoint (FastAPI + SQLite).
- Data validation + tests: `KBBQ/Validate Data (Portfolio)` and Unity EditMode tests for math invariants and save integrity.
- Deterministic simulator (`sim/`): .NET unit tests for economy/progression math.
- WebGL build to `docs/` for GitHub Pages (`KBBQ/Build WebGL (docs)`).

Simulator test baseline: .NET SDK `10.x`.

Service spec/execution docs:
- `docs/SPECKIT_SERVICE_100.ko.md`
- `docs/COT_EXECUTION_LOG.ko.md`

Ops/deploy helpers:
- `server/.env.production.example`, `server/.env.staging.example`
- `tools/deploy_backend.sh`
- `tools/check_backend_ops.sh`
- `tools/backup_kbbq_db.sh`

## Portfolio Quality Gate
Run a single local gate before demo/review:

```bash
tools/portfolio_quality_gate.sh
```

This executes simulator tests, backend tests, and Unity local CI checks.

Notes:
- If `.NET SDK` is missing, sim tests are skipped by default.
- Use `STRICT_PORTFOLIO_GATE=1 tools/portfolio_quality_gate.sh` for strict failure on missing gates.
