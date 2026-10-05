#!/usr/bin/env bash
# Runs the instructor PlayMode tests (Assets/_Instructor/Tests) on the project as it is now.
# They play a level through a virtual gamepad; tests for things that don't exist yet
# at the current checkpoint are skipped. Close the Unity Editor first.
#
#   Tools/run-tests.sh [name] [scenes]   # scenes: Arena (default), "all" (Arena + every checkpoint scene), or "A;B"
set -euo pipefail
source "$(dirname "$0")/common.sh"

NAME="${1:-tests}"
SCENES="${2:-Arena}"
RESULTS="$LOG_DIR/$NAME-results.xml"
mkdir -p "$LOG_DIR"
rm -f "$RESULTS"

set +e
run_unity "$NAME" -runTests -testPlatform PlayMode -testResults "$(native_path "$RESULTS")" -testScenes "$SCENES"
code=$?
set -e

if [[ ! -f "$RESULTS" ]]; then
  echo "No test results. See $LOG_DIR/$NAME.log" >&2
  exit 1
fi
grep -o '<test-run [^>]*' "$RESULTS" | grep -o -E '(passed|failed|skipped)="[0-9]+"' | tr '\n' ' '
echo
grep -o -E '<test-case [^>]*result="Failed"[^>]*>' "$RESULTS" | sed -E 's/.* name="([^"]*)".*/  FAILED: \1/' || true
exit $code
