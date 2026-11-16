#!/usr/bin/env bash
set -Eeuo pipefail
# Run once from the deployment pipeline before scaling up new API replicas.
docker compose run --build --rm api --migrate-only
