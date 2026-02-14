# KBBQ Idle (Unity) — Project Summary

## High-level
- Unity 2022.3.62f3 LTS project with a single scene: `Assets/Scenes/Main.unity`.
- Idle/tycoon-style K-BBQ restaurant game: earn passive income, serve customers, upgrade systems, unlock menu/store tiers, and prestige for long‑term progression.

## Core gameplay loop
- **Economy**: `EconomySystem` computes income/sec from menu items, upgrade multipliers (income/menu/staff/service/sizzle), store tier multiplier, boosts, tips, combos, and prestige.
- **Customers & queue**: `CustomerSystem` spawns a queue, tracks satisfaction and patience, auto-serves over time, and supports manual “Serve” for tips + combo. Rush service temporarily speeds service.
- **Progression**: player level advances based on total income (`ProgressionSystem` + `EconomyTuning`); menu items and store tiers unlock by level.
- **Prestige**: `PrestigeSystem` grants prestige points (multiplier) and resets progress when eligible.
- **Sessions**: offline earnings (60% rate, capped at 8 hours), daily login rewards, and daily missions (earn currency / use boost / purchase upgrades).
- **Tutorial**: 3-step flow (boost → upgrade → serve).

## Architecture & key systems
- **Orchestrator**: `GameManager` initializes systems, drives the tick loop, persists saves, and updates UI.
- **Save**: `SaveSystem` stores `SaveData` in PlayerPrefs (`KBBQ_IDLE_SAVE`).
- **State**: `GameStateMachine` (Boot/Tutorial/MainLoop/Pause/OfflineCalc).
- **UI**: `UIController` binds views for missions, prestige, queue, upgrades, debug/perf overlay, tutorial, leaderboard, and monetization.
- **Analytics**: `AnalyticsService` logs events to console and can optionally forward events to the local backend (best-effort) when networking is enabled.

## Data & content
- ScriptableObject assets in `Assets/Data`:
  - Menu items, upgrades, store tiers, customer types.
  - Config: `GameDataCatalog`, `EconomyTuning`, `MonetizationConfig`, `ApiConfig`.
- Example content:
  - Menu items: Pork Belly, Beef Brisket, Soju, Bingsu, etc.
  - Upgrades: Grill Upgrade, Ventilation, recipe upgrades, etc.
  - Store tiers: Alley → Hongdae → Gangnam → Hanok → Global.
- `GameDataCatalog` ties assets together; `DefaultDataFactory` provides fallback data if assets are missing.

## Monetization
- `MonetizationConfig` defines rewarded boost, interstitial reward, and IAP packs.
- `MonetizationService` grants currency/boosts directly (no real ad/IAP SDK wired in).

## Networking & backend (optional)
- Unity network clients (`AuthClient`, `LeaderboardClient`, `FriendsClient`, `AnalyticsClient`) use `UnityWebRequest` with HMAC-signed headers + nonce replay protection.
- `ApiConfig` gates network usage; networking ships **disabled by default** for safety (`enableNetwork=false`).
- When networking is enabled and the backend is running, `LeaderboardView` will:
  - ensure guest auth,
  - submit the current score (best-effort),
  - fetch the live top list (fallbacks to mock data on failure).
- A small local backend is included under `server/` (FastAPI + SQLite) implementing:
  - guest auth: `POST /auth/guest`
  - leaderboard: `POST /leaderboard/submit`, `GET /leaderboard/top`
  - friends: `POST /friends/invite`, `GET /friends/list`
  - analytics: `POST /analytics/event`

## Portfolio hardening (quality signals)
- **Data validation**: `KBBQ/Validate Data (Portfolio)` checks duplicate IDs, tuning sanity, and safe network defaults.
- **Unity tests**: EditMode tests cover math invariants + save integrity guards.
- **WebGL pipeline**: `KBBQ/Build WebGL (docs)` generates build output into `docs/` for GitHub Pages (build artifacts generated locally to keep the repo lightweight).
- `Assets/Editor/KBBQAutoSetup.cs` can auto-create folders, data assets, prefabs, and `Main.unity` via menu **KBBQ/Run Auto Setup**.
  - Build/cache artifacts (e.g., `Library/`, `Builds/`) are intentionally excluded from this repo.

## Gaps / notes
- Main documentation is in `README.md` (with `README.en.md` / `README.ko.md`).
- Tests are added/extended as part of the portfolio hardening work.
