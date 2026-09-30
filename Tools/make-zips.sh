#!/usr/bin/env bash
# Makes one attendee zip per checkpoint tag in dist/, straight from git (no Library folder,
# no local changes). Instructor-only files are left out via export-ignore in .gitattributes:
# Assets/_Instructor (builder + tests), Tools/, INSTRUCTOR_GUIDE.md and MANUAL_SETUP.md.
#
#   Tools/make-zips.sh
# Then attach dist/*.zip to a GitHub Release (see Docs/MANUAL_SETUP.md).
set -euo pipefail
source "$(dirname "$0")/common.sh"

mkdir -p "$REPO_ROOT/dist"
for tag in "${CHECKPOINTS[@]}"; do
  if ! git -C "$REPO_ROOT" rev-parse -q --verify "refs/tags/$tag" >/dev/null; then
    echo "skip $tag (no such tag)"
    continue
  fi
  zip="$REPO_ROOT/dist/unity-workshop-$tag.zip"
  # No top-level folder inside the zip: Windows "Extract All" and macOS both create one named after
  # the zip, and a second nested level would push Unity's deep package paths past Windows' 260-char limit.
  git -C "$REPO_ROOT" archive --format=zip -o "$zip" "$tag"
  if unzip -l "$zip" | grep -q -E "_Instructor|INSTRUCTOR_GUIDE|Tools/"; then
    echo "ERROR: instructor files leaked into $zip" >&2
    exit 1
  fi
  echo "$(du -h "$zip" | cut -f1)  dist/unity-workshop-$tag.zip"
done
