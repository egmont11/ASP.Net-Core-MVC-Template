#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: $0 <NewProjectName>"
  exit 1
fi

NEW_NAME="$1"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$SCRIPT_DIR/Template"

if [[ ! -d "$ROOT" ]]; then
  echo "Error: Template directory not found at $ROOT"
  exit 1
fi

echo "Renaming 'Template' -> '$NEW_NAME' in: $ROOT"

# Rename files first (deepest first to avoid path issues)
while IFS= read -r -d '' path; do
  dir="$(dirname "$path")"
  base="$(basename "$path")"
  new_base="${base//Template/$NEW_NAME}"
  if [[ "$base" != "$new_base" ]]; then
    echo "  FILE: $base -> $new_base"
    mv "$path" "$dir/$new_base"
  fi
done < <(find "$ROOT" \
  -not -path "*/bin/*" \
  -not -path "*/obj/*" \
  -not -path "*/.idea/*" \
  -type f -name "*Template*" -print0 | sort -rz)

# Rename directories (deepest first)
while IFS= read -r -d '' path; do
  parent="$(dirname "$path")"
  base="$(basename "$path")"
  new_base="${base//Template/$NEW_NAME}"
  if [[ "$base" != "$new_base" ]]; then
    echo "  DIR:  $base -> $new_base"
    mv "$path" "$parent/$new_base"
  fi
done < <(find "$ROOT" \
  -not -path "*/bin/*" \
  -not -path "*/obj/*" \
  -not -path "*/.idea/*" \
  -type d -name "*Template*" -print0 | sort -rz)

echo "Done."
