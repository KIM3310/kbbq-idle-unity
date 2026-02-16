[Personal Project] KBBQ Idle — Unity Idle/Tycoon + Secure Backend Sandbox

KBBQ Idle is a Unity (2022.3 LTS) idle/tycoon game where you run a K‑BBQ restaurant and grow through upgrades, queue management, offline earnings, daily missions, and prestige resets.

I used this project as an “engineering rigor” sandbox beyond gameplay, focusing on reliability, security, and reviewable math:

- Save integrity: versioned PlayerPrefs JSON + SHA‑256 checksum guard + defensive sanitization/clamping to handle corruption/tampering
- Deterministic economy harness: extracted progression formulas into a small .NET simulator with unit tests so balance changes are reviewable without launching Unity
- Data validation: Unity Editor validator + EditMode tests to catch duplicate ScriptableObject IDs, invalid tuning values, and unsafe config early
- Optional backend demo (FastAPI + SQLite): guest auth, leaderboard/friends endpoints, and lightweight analytics event ingestion
- HMAC‑signed requests (timestamp + nonce) with replay protection; tokens stored as SHA‑256 hashes (no raw tokens)
- Networking is opt‑in by design (ships disabled by default for safety)

GitHub:
https://github.com/KIM3310/kbbq-idle-unity

