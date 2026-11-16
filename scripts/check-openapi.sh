#!/usr/bin/env bash
set -Eeuo pipefail
: "${API_BASE_URL:?Set API_BASE_URL to the running API base URL}"
[[ -f openapi/v1.json ]] || { echo "openapi/v1.json is missing; run scripts/export-openapi.sh" >&2; exit 1; }
ACTUAL="$(mktemp)"
EXPECTED="$(mktemp)"
trap 'rm -f "$ACTUAL" "$EXPECTED"' EXIT
curl --fail --silent --show-error "$API_BASE_URL/openapi/v1.json" | jq -S 'del(.servers)' > "$ACTUAL"
jq -S 'del(.servers)' openapi/v1.json > "$EXPECTED"
if ! diff -u "$EXPECTED" "$ACTUAL"; then
  echo "The served API OpenAPI document differs from openapi/v1.json." >&2
  echo "Regenerate with: API_BASE_URL=http://localhost:8080 ./scripts/export-openapi.sh" >&2
  exit 1
fi
echo "Served OpenAPI document matches openapi/v1.json"
