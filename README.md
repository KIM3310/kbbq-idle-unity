# KBBQ Idle — Unity 2022 Idle/Tycoon (Personal Project, 2022)

An idle/tycoon-style K-BBQ restaurant game built in Unity (2022.3 LTS). The gameplay centers on passive income, upgrades, queue management, unlock progression, and prestige for long-term growth.

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
- Save: PlayerPrefs JSON (`KBBQ_IDLE_SAVE`) with sanity checks
- Economy: menu income x upgrades x store tier x boosts x tips/combos x prestige
- Sessions: offline earnings, daily login, daily missions
- Networking/monetization: structured as stubs (no real SDK wired by default)

## Deterministic Simulator (Tests)
To keep the math reviewable outside Unity, `sim/` contains a small .NET project with unit tests for the economy/progression formulas.

```bash
dotnet test sim/KbbqIdle.Sim.Tests/KbbqIdle.Sim.Tests.csproj
```
