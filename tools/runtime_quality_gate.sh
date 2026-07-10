#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PYTHON_BIN="${PYTHON_BIN:-python3}"
VENV_DIR="${VENV_DIR:-$ROOT_DIR/.venv}"
STRICT_RUNTIME_GATE="${STRICT_RUNTIME_GATE:-0}"

echo "[RUNTIME] Root: $ROOT_DIR"

if command -v dotnet >/dev/null 2>&1; then
  echo "[RUNTIME] Running deterministic sim tests..."
  dotnet test "$ROOT_DIR/sim/KbbqIdle.Sim.Tests/KbbqIdle.Sim.Tests.csproj"
else
  if [ "$STRICT_RUNTIME_GATE" = "1" ]; then
    echo "[RUNTIME] dotnet not found. Install .NET 8 SDK to run sim tests."
    exit 2
  fi
  echo "[RUNTIME] dotnet not found. Skipping sim tests (set STRICT_RUNTIME_GATE=1 to fail instead)."
fi

if [ ! -x "$VENV_DIR/bin/python" ]; then
  echo "[RUNTIME] Creating Python venv at $VENV_DIR"
  "$PYTHON_BIN" -m venv "$VENV_DIR"
fi

echo "[RUNTIME] Installing backend test dependencies..."
"$VENV_DIR/bin/pip" install -q -e "${ROOT_DIR}[dev]"

echo "[RUNTIME] Running backend tests..."
"$VENV_DIR/bin/python" -m pytest "$ROOT_DIR/server/tests" -q

echo "[RUNTIME] Running Unity validation (EditMode/PlayMode/Data Validator)..."
"$ROOT_DIR/tools/ci_unity_checks.sh" "$ROOT_DIR"

echo "[RUNTIME] All quality gates passed."
