#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_PATH="${1:-$ROOT_DIR}"
UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity}"
OUT_DIR="${OUT_DIR:-$PROJECT_PATH/Logs/ci}"

mkdir -p "$OUT_DIR"

if [ ! -x "$UNITY_BIN" ]; then
  echo "[CI] Unity binary not found or not executable: $UNITY_BIN"
  exit 2
fi

run_tests() {
  local platform="$1"
  local slug="$2"
  local xml_path="$OUT_DIR/${slug}_results.xml"
  local log_path="$OUT_DIR/${slug}.log"

  echo "[CI] Running ${platform} tests..."
  "$UNITY_BIN" \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -runTests \
    -testPlatform "$platform" \
    -testResults "$xml_path" \
    -logFile "$log_path"

  python3 - "$xml_path" "$platform" <<'PY'
import sys
import xml.etree.ElementTree as ET

xml_path, platform = sys.argv[1], sys.argv[2]
root = ET.parse(xml_path).getroot()
result = root.attrib.get("result", "")
total = root.attrib.get("total", "0")
passed = root.attrib.get("passed", "0")
failed = root.attrib.get("failed", "0")
print(f"[CI] {platform}: result={result} total={total} passed={passed} failed={failed}")
if not result.startswith("Passed"):
    sys.exit(1)
PY
}

run_validation() {
  local log_path="$OUT_DIR/validate.log"
  echo "[CI] Running validator..."
  "$UNITY_BIN" \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -executeMethod KBBQDataValidator.ValidateMenu \
    -quit \
    -logFile "$log_path"

  if ! rg -q "KBBQ Validate: errors=0, warnings=0" "$log_path"; then
    echo "[CI] Validator failed. Check log: $log_path"
    exit 1
  fi

  echo "[CI] Validator: errors=0 warnings=0"
}

run_tests "EditMode" "editmode"
run_tests "PlayMode" "playmode"
run_validation

echo "[CI] All Unity checks passed."
