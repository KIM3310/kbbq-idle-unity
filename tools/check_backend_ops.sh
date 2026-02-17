#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${1:-http://127.0.0.1:8000}"
OPS_TOKEN="${KBBQ_OPS_ADMIN_TOKEN:-}"

echo "[OPS] base_url=$BASE_URL"
echo "[OPS] health:"
curl -sS --max-time 5 "$BASE_URL/health" | python3 -m json.tool

echo "[OPS] readiness:"
READINESS_JSON="$(curl -sS --max-time 5 "$BASE_URL/readiness")"
echo "$READINESS_JSON" | python3 -m json.tool

READY="$(echo "$READINESS_JSON" | python3 -c 'import sys,json; d=json.load(sys.stdin); print("1" if d.get("ready") else "0")')"
if [ "$READY" != "1" ]; then
  echo "[OPS] readiness is false"
  exit 1
fi

echo "[OPS] metrics sample:"
curl -sS --max-time 5 "$BASE_URL/metrics" | head -n 20

if [ -n "$OPS_TOKEN" ]; then
  echo "[OPS] alerts:"
  curl -sS --max-time 5 "$BASE_URL/ops/alerts" -H "X-Ops-Token: $OPS_TOKEN" | python3 -m json.tool
else
  echo "[OPS] KBBQ_OPS_ADMIN_TOKEN not set, skip /ops/alerts."
fi

echo "[OPS] checks completed"
