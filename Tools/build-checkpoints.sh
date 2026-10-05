#!/usr/bin/env bash
# Rebuilds every workshop checkpoint in order, then commits and tags each one:
#   1. writes that step's lesson scripts from Docs/INSTRUCTOR_GUIDE.md
#   2. runs the Unity builder (Workshop.Instructor.WorkshopMenu.BuildFromCommandLine),
#      which wires scenes/prefabs and validates the result
#   3. runs the PlayMode tests
#   4. git commit + git tag -f <checkpoint>
# Close the Unity Editor first. Tags are moved with -f, so push them with --force if you publish.
#
#   Tools/build-checkpoints.sh                         # everything, from the starter
#   Tools/build-checkpoints.sh --from step-5-hazards   # resume from a checkpoint
#   Tools/build-checkpoints.sh --no-tests --no-commit  # just rebuild the project files
# Set COMMIT_TRAILER="Co-Authored-By: ..." to append a trailer to each commit message.
set -euo pipefail
source "$(dirname "$0")/common.sh"

FROM="step-0-starter"
TESTS=1
COMMIT=1
while [[ $# -gt 0 ]]; do
  case "$1" in
    --from) FROM="$2"; shift 2 ;;
    --no-tests) TESTS=0; shift ;;
    --no-commit) COMMIT=0; shift ;;
    *) echo "Unknown option $1" >&2; exit 1 ;;
  esac
done
FROM_INDEX="$(checkpoint_index "$FROM")"

reset_to_starter() {
  # Remove everything the lesson creates. Scenes, materials and _Backup are rebuilt in place.
  local w="$PROJECT_DIR/Assets/_Workshop"
  find "$w/Scripts" -mindepth 1 ! -name 'HelloWorld.cs' ! -name 'HelloWorld.cs.meta' -exec rm -rf {} +
  find "$w/Prefabs" "$w/PhysicsMaterials" -mindepth 1 ! -name '.gitkeep' -exec rm -rf {} +
  # The checkpoint scenes are regenerated from scratch by the starter build.
  rm -rf "$w/Checkpoints" "$w/Checkpoints.meta"
  "$REPO_ROOT/Tools/extract-lesson-scripts.sh" --checkpoint-copies
}

for i in "${!CHECKPOINTS[@]}"; do
  (( i < FROM_INDEX )) && continue
  tag="${CHECKPOINTS[$i]}"
  echo "=== $tag ==="

  if [[ "$tag" == "step-0-starter" ]]; then
    reset_to_starter
  fi
  "$REPO_ROOT/Tools/extract-lesson-scripts.sh" "$tag"

  if ! run_unity "build-$tag" -executeMethod Workshop.Instructor.WorkshopMenu.BuildFromCommandLine -checkpoint "$tag"; then
    echo "Build of $tag failed. See $LOG_DIR/build-$tag.log" >&2
    grep -E "\[Workshop\]|error CS|Exception" "$LOG_DIR/build-$tag.log" >&2 || true
    exit 1
  fi
  if grep -E "warning CS|error CS" "$LOG_DIR/build-$tag.log" | grep -v "Library/PackageCache" >&2; then
    echo "Compiler warnings in $tag (see above). The scripts must compile cleanly." >&2
    exit 1
  fi

  if (( TESTS )); then
    # The checkpoint scenes only change with the starter, so play all of them once there.
    scenes="Arena"
    [[ "$tag" == "step-0-starter" ]] && scenes="all"
    "$REPO_ROOT/Tools/run-tests.sh" "tests-$tag" "$scenes"
  fi

  if (( COMMIT )); then
    git -C "$REPO_ROOT" add -A
    if git -C "$REPO_ROOT" diff --cached --quiet; then
      echo "No changes for $tag; tagging the current commit."
    else
      git -C "$REPO_ROOT" commit -q -m "Checkpoint: $tag" ${COMMIT_TRAILER:+-m "$COMMIT_TRAILER"}
    fi
    git -C "$REPO_ROOT" tag -f "$tag" >/dev/null
    echo "Tagged $tag"
  fi
done
echo "Done. Next: Tools/make-zips.sh"
