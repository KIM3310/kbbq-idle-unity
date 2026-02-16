# KBBQ Idle — Unity 2022 Idle/Tycoon (Personal Project, 2022)

An idle/tycoon-style K-BBQ restaurant game built in Unity (2022.3 LTS). The gameplay centers on passive income, upgrades, queue management, unlock progression, and prestige for long-term growth.

License: MIT (see `LICENSE`). Third-party notices: `THIRD_PARTY_NOTICES.md`.

## Docs
- English: `README.en.md`
- Korean: `README.ko.md`
- Deeper technical summary: `PROJECT_SUMMARY.en.md`, `PROJECT_SUMMARY.ko.md`

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
- Networking/monetization: structured as stubs (no real SDK wired by default)

## Deterministic Simulator (Tests)
To keep the math reviewable outside Unity, `sim/` contains a small .NET project with unit tests for the economy/progression formulas.

```bash
dotnet test sim/KbbqIdle.Sim.Tests/KbbqIdle.Sim.Tests.csproj
```

## Optional Backend (Leaderboard + Friends)
This repo includes a small FastAPI + SQLite backend in `server/` that matches the Unity network clients:
- `POST /auth/guest` (guest auth)
- `POST /leaderboard/submit`, `GET /leaderboard/top`
- `POST /analytics/event` (lightweight event ingestion)
- `POST /friends/invite`, `GET /friends/list`

Run with Docker:
```bash
export KBBQ_HMAC_SECRET="CHANGE_ME"
docker compose up --build
```
Details: `server/README.md`.

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

## Glossary (first-time readers)
- HMAC: Hash-based Message Authentication Code (request signing)
- SHA-256: Secure Hash Algorithm 256-bit (checksum / integrity)
