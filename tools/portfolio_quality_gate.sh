#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PYTHON_BIN="${PYTHON_BIN:-python3}"
VENV_DIR="${VENV_DIR:-$ROOT_DIR/.venv}"
STRICT_PORTFOLIO_GATE="${STRICT_PORTFOLIO_GATE:-0}"

echo "[PORTFOLIO] Root: $ROOT_DIR"

if command -v dotnet >/dev/null 2>&1; then
  echo "[PORTFOLIO] Running deterministic sim tests..."
  dotnet test "$ROOT_DIR/sim/KbbqIdle.Sim.Tests/KbbqIdle.Sim.Tests.csproj"
else
  if [ "$STRICT_PORTFOLIO_GATE" = "1" ]; then
    echo "[PORTFOLIO] dotnet not found. Install .NET 8 SDK to run sim tests."
    exit 2
  fi
  echo "[PORTFOLIO] dotnet not found. Skipping sim tests (set STRICT_PORTFOLIO_GATE=1 to fail instead)."
fi

if [ ! -x "$VENV_DIR/bin/python" ]; then
  echo "[PORTFOLIO] Creating Python venv at $VENV_DIR"
  "$PYTHON_BIN" -m venv "$VENV_DIR"
fi

echo "[PORTFOLIO] Installing backend test dependencies..."
"$VENV_DIR/bin/pip" install -q -r "$ROOT_DIR/server/requirements.txt"

echo "[PORTFOLIO] Running backend tests..."
"$VENV_DIR/bin/python" -m pytest "$ROOT_DIR/server/tests" -q

echo "[PORTFOLIO] Running Unity validation (EditMode/PlayMode/Data Validator)..."
"$ROOT_DIR/tools/ci_unity_checks.sh" "$ROOT_DIR"

echo "[PORTFOLIO] All quality gates passed."
