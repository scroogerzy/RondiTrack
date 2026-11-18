#!/usr/bin/env bash
set -Eeuo pipefail
: "${API_BASE_URL:?Set API_BASE_URL to the running API base URL}"
mkdir -p openapi
TMP="$(mktemp)"
trap 'rm -f "$TMP"' EXIT
curl --fail --silent --show-error "$API_BASE_URL/openapi/v1.json" -o "$TMP"
jq -S 'del(.servers)' "$TMP" > openapi/v1.json
printf 'Updated openapi/v1.json from %s\n' "$API_BASE_URL"
