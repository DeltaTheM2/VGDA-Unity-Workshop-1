#!/usr/bin/env bash
# Shared settings for the Tools/ scripts. Source it; don't run it.

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_DIR="$REPO_ROOT/UnityProject"
GUIDE="$REPO_ROOT/Docs/INSTRUCTOR_GUIDE.md"
LOG_DIR="$PROJECT_DIR/Logs/workshop-tools"

# Every checkpoint, in order. Step 4 (Remix #1) has no checkpoint.
CHECKPOINTS=(step-0-starter step-1-lifecycle step-2-player step-3-prefabs step-5-hazards step-6-goal final)

UNITY_VERSION="$(sed -n 's/^m_EditorVersion: //p' "$PROJECT_DIR/ProjectSettings/ProjectVersion.txt" | tr -d '\r')"

# Override with: UNITY=/path/to/Unity ./Tools/build-checkpoints.sh
if [[ -z "${UNITY:-}" ]]; then
  case "$(uname -s)" in
    Darwin) UNITY="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity" ;;
    Linux)  UNITY="$HOME/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity" ;;
    *)      UNITY="/c/Program Files/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity.exe" ;;
  esac
fi

# Paths handed to Unity itself must be native (C:/... on Windows).
native_path() {
  if command -v cygpath >/dev/null 2>&1; then cygpath -m "$1"; else echo "$1"; fi
}

# Index of a checkpoint tag in CHECKPOINTS, or exit if unknown.
checkpoint_index() {
  local i
  for i in "${!CHECKPOINTS[@]}"; do
    [[ "${CHECKPOINTS[$i]}" == "$1" ]] && { echo "$i"; return 0; }
  done
  echo "Unknown checkpoint '$1'. Known: ${CHECKPOINTS[*]}" >&2
  exit 1
}

# run_unity <log-name> <unity args...>
run_unity() {
  local name="$1"; shift
  mkdir -p "$LOG_DIR"
  if [[ ! -x "$UNITY" && ! -f "$UNITY" ]]; then
    echo "Unity $UNITY_VERSION not found at: $UNITY (set UNITY=/path/to/Unity)" >&2
    exit 1
  fi
  "$UNITY" -batchmode -projectPath "$(native_path "$PROJECT_DIR")" -logFile "$(native_path "$LOG_DIR/$name.log")" "$@"
}
