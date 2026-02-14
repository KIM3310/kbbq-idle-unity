#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

UNITY_PATH_DEFAULT="/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
UNITY="${UNITY_PATH:-$UNITY_PATH_DEFAULT}"

if [[ ! -x "$UNITY" ]]; then
  # Try auto-discovery (Unity Hub default install location on macOS).
  candidates=(/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity)
  if [[ ${#candidates[@]} -gt 0 ]]; then
    for (( i=${#candidates[@]}-1; i>=0; i-- )); do
      if [[ -x "${candidates[$i]}" ]]; then
        UNITY="${candidates[$i]}"
        break
      fi
    done
  fi
fi

if [[ ! -x "$UNITY" ]]; then
  echo "Unity executable not found."
  echo "Set UNITY_PATH to your Unity binary, e.g.:"
  echo "  export UNITY_PATH=\"/path/to/Unity.app/Contents/MacOS/Unity\""
  exit 1
fi

echo "Building WebGL into docs/ (GitHub Pages)..."
echo "Unity: $UNITY"
echo "Project: $PROJECT_ROOT"

"$UNITY" \
  -quit -batchmode \
  -projectPath "$PROJECT_ROOT" \
  -executeMethod KBBQWebGLBuild.BuildWebGLDocsCLI \
  -logFile "$PROJECT_ROOT/build_webgl.log"

echo "Done. Output: $PROJECT_ROOT/docs"
