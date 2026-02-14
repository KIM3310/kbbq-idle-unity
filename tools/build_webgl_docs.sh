#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

UNITY_PATH_DEFAULT="/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
UNITY="${UNITY_PATH:-$UNITY_PATH_DEFAULT}"

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

