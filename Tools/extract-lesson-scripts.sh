#!/usr/bin/env bash
# Writes the lesson scripts for a checkpoint straight out of Docs/INSTRUCTOR_GUIDE.md,
# so the code in the guide and the code in the project can never drift apart.
#
#   Tools/extract-lesson-scripts.sh step-6-hazards          # write every script up to and including Step 6
#   Tools/extract-lesson-scripts.sh step-6-hazards --check  # only compare, exit 1 if the project differs
#   Tools/extract-lesson-scripts.sh --checkpoint-copies     # write the copies the checkpoint scenes use
#
# In the guide, a script is a ```csharp block right after a marker line like:
#   <!-- lesson-script: step-4-player Assets/_Workshop/Scripts/PlayerController.cs -->
#
# Checkpoint copies go to Assets/_Workshop/Checkpoints/_Scripts, wrapped in namespace
# WorkshopCheckpoints and hidden from Add Component, so they never clash with the
# scripts attendees type themselves (same class names, different namespace).
set -euo pipefail
source "$(dirname "$0")/common.sh"

if [[ "${1:-}" == "--checkpoint-copies" ]]; then
  TARGET="final"
  MODE="copies"
else
  TARGET="${1:?usage: extract-lesson-scripts.sh <checkpoint-tag> [--check] | --checkpoint-copies}"
  MODE="${2:-write}"
fi
TARGET_INDEX="$(checkpoint_index "$TARGET")"

ALLOWED=""
for i in "${!CHECKPOINTS[@]}"; do
  (( i <= TARGET_INDEX )) && ALLOWED="$ALLOWED ${CHECKPOINTS[$i]} "
done

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

# Pull every allowed block into TMP_DIR/<path>.
awk -v allowed="$ALLOWED" -v out="$TMP_DIR" '
  /<!-- lesson-script: / {
    tag = $3; path = $4
    want = index(allowed, " " tag " ") > 0
    next
  }
  want && /^```csharp/ { capturing = 1; file = out "/" path; dir = file; sub(/\/[^\/]*$/, "", dir); system("mkdir -p \"" dir "\""); printf "" > file; next }
  capturing && /^```/ { capturing = 0; want = 0; close(file); next }
  capturing { print >> file }
' "$GUIDE"

if [[ "$MODE" == "copies" ]]; then
  COPIES="$PROJECT_DIR/Assets/_Workshop/Checkpoints/_Scripts"
  mkdir -p "$COPIES"
  # Every lesson script, plus the one bonus script the demo course uses.
  for file in "$TMP_DIR"/Assets/_Workshop/Scripts/*.cs "$TMP_DIR"/Assets/_Workshop/Scripts/Bonus/BouncePad.cs; do
    name="$(basename "$file")"
    awk '
      BEGIN { print "// Checkpoint copy written by Tools/extract-lesson-scripts.sh from Docs/INSTRUCTOR_GUIDE.md. Edit the guide, not this file." }
      !started && (/^using / || /^$/) { print; next }
      !started { started = 1; print "namespace WorkshopCheckpoints"; print "{" }
      /^public class / { print "    [AddComponentMenu(\"\")]" }
      { if ($0 == "") print ""; else print "    " $0 }
      END { print "}" }
    ' "$file" > "$COPIES/$name"
    echo "wrote checkpoint copy $name"
  done
  exit 0
fi

status=0
while IFS= read -r -d '' file; do
  rel="${file#"$TMP_DIR"/}"
  dest="$PROJECT_DIR/$rel"
  if [[ "$MODE" == "--check" ]]; then
    if ! diff -q "$file" "$dest" >/dev/null 2>&1; then
      echo "DIFFERS from the guide: $rel"
      status=1
    fi
  else
    mkdir -p "$(dirname "$dest")"
    cp "$file" "$dest"
    echo "wrote $rel"
  fi
done < <(find "$TMP_DIR" -type f -print0 | sort -z)

if [[ "$MODE" == "--check" && $status -eq 0 ]]; then
  echo "All lesson scripts up to $TARGET match the guide."
fi
exit $status
