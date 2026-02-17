#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DB_PATH="${KBBQ_DB_PATH:-$ROOT_DIR/kbbq.db}"
BACKUP_DIR="${1:-$ROOT_DIR/backups/db}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"

if [ ! -f "$DB_PATH" ]; then
  echo "[BACKUP] db file not found: $DB_PATH"
  exit 2
fi

mkdir -p "$BACKUP_DIR"
STAMP="$(date -u +%Y%m%dT%H%M%SZ)"
TARGET="$BACKUP_DIR/kbbq_${STAMP}.db"

cp "$DB_PATH" "$TARGET"
gzip -f "$TARGET"

find "$BACKUP_DIR" -type f -name "kbbq_*.db.gz" -mtime "+$RETENTION_DAYS" -delete

echo "[BACKUP] created $TARGET.gz"
