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

## Common Issues
- Scene missing bindings:
  - Run **KBBQ/Run Auto Setup** (Editor menu) to regenerate data assets and UI prefabs.
- Networking warnings:
  - Networking is disabled by default in this repo. If you enabled it, set a real base URL and secret in `Assets/Data/Config/ApiConfig.asset`.

