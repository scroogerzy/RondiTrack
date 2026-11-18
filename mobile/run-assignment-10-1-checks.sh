#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
flutter pub get
dart run build_runner build
dart format --output=none --set-exit-if-changed .
dart analyze --fatal-infos
flutter test
