# KBBQ Idle (Unity 2022)

## Overview
This is a Unity `2022.3.62f3` LTS project for an idle/tycoon K‑BBQ restaurant game. The project is built around a single scene at `Assets/Scenes/Main.unity` and focuses on passive income, upgrades, and long‑term progression.

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
- Monetization rewards and IAP packs are configured but use stub services (no real SDK wired).
- Optional network clients exist (HMAC‑signed headers) but the base URL is a placeholder, and leaderboards use mock data by default.

## Quick Start
- Open with Unity `2022.3.62f3`.
- Load `Assets/Scenes/Main.unity` and press Play.
- Optional: run **KBBQ/Run Auto Setup** to regenerate data assets and UI prefabs.

## Portfolio Add-ons
- Optional backend (`server/`): guest auth + HMAC-signed leaderboard/friends endpoints (FastAPI + SQLite).
- Deterministic simulator (`sim/`): .NET unit tests for economy/progression math.
- WebGL build to `docs/` for GitHub Pages (`KBBQ/Build WebGL (docs)`).
