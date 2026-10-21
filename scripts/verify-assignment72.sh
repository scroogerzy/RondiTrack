#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# The incoming web lockfile predates the pinned Assignment 7.2 packages.
# npm install resolves that lockfile once; after it is regenerated use npm ci.
cd "$ROOT_DIR/web"
npm install --no-audit --no-fund
npm run typecheck
npm run lint
npm run build
npx vitest run
npm audit
npm audit --omit=dev

cd "$ROOT_DIR"
dotnet test RondiTrack.sln
