#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="${1:-$ROOT_DIR/server/.env.production.example}"
BASE_URL="${2:-http://127.0.0.1:8000}"

if [ ! -f "$ENV_FILE" ]; then
  echo "[DEPLOY] env file not found: $ENV_FILE"
  exit 2
fi

echo "[DEPLOY] root=$ROOT_DIR"
echo "[DEPLOY] env_file=$ENV_FILE"
echo "[DEPLOY] base_url=$BASE_URL"

set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

cd "$ROOT_DIR"
docker compose up -d --build

echo "[DEPLOY] waiting for /health ..."
for i in {1..30}; do
  if curl -sS --max-time 3 "$BASE_URL/health" >/dev/null; then
    break
  fi
  sleep 2
done

echo "[DEPLOY] health:"
curl -sS --max-time 5 "$BASE_URL/health" | python3 -m json.tool

echo "[DEPLOY] readiness:"
curl -sS --max-time 5 "$BASE_URL/readiness" | python3 -m json.tool

echo "[DEPLOY] deployment completed"
