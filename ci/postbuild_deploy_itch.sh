#!/usr/bin/env bash
set -euo pipefail

# Required env vars
: "${BUTLER_API_KEY:?Missing BUTLER_API_KEY}"
: "${ITCH_TARGET:?Missing ITCH_TARGET (e.g. username/game)}"
: "${ITCH_CHANNEL:?Missing ITCH_CHANNEL (e.g. html5)}"

# Artifact path from UBA
ARTIFACT_PATH="${UNITY_PLAYER_PATH:-${3:-}}"
if [[ -z "$ARTIFACT_PATH" ]]; then
  echo "ERROR: No artifact path (UNITY_PLAYER_PATH empty and arg #3 missing)."
  exit 1
fi

echo "BUILDER_OS=${BUILDER_OS:-unknown}"
echo "UNITY_PLAYER_PATH=${UNITY_PLAYER_PATH:-}"
echo "Artifact path (raw)=$ARTIFACT_PATH"

# Choose correct butler platform for broth
BUTLER_PLATFORM="linux-amd64"
if [[ "${BUILDER_OS:-}" == "WINDOWS" ]]; then
  BUTLER_PLATFORM="windows-amd64"
elif [[ "${BUILDER_OS:-}" == "MAC" ]]; then
  # If you ever switch to a macOS builder:
  if [[ "$(uname -m 2>/dev/null || echo amd64)" == "arm64" ]]; then
    BUTLER_PLATFORM="darwin-arm64"
  else
    BUTLER_PLATFORM="darwin-amd64"
  fi
fi

echo "Downloading butler platform: $BUTLER_PLATFORM"
curl -L -o butler.zip "https://broth.itch.zone/butler/${BUTLER_PLATFORM}/LATEST/archive/default"

# Extract 
PYTHON_BIN="python3"
command -v "$PYTHON_BIN" >/dev/null 2>&1 || PYTHON_BIN="python"
command -v "$PYTHON_BIN" >/dev/null 2>&1 || { echo "ERROR: python is required to extract butler.zip"; exit 1; }

"$PYTHON_BIN" - <<'PY'
import zipfile, os
os.makedirs("butler_bin", exist_ok=True)
with zipfile.ZipFile("butler.zip", "r") as z:
    z.extractall("butler_bin")
PY

# Find the butler executable
if [[ "${BUILDER_OS:-}" == "WINDOWS" ]]; then
  BUTLER="$(find butler_bin -maxdepth 2 -type f -iname 'butler*.exe' | head -n 1)"
else
  BUTLER="$(find butler_bin -maxdepth 2 -type f -iname 'butler*' | head -n 1)"
fi
if [[ -z "$BUTLER" ]]; then
  echo "ERROR: could not find butler after extraction"
  exit 1
fi
chmod +x "$BUTLER" || true

# Convert artifact path for Windows native executable
PUSH_PATH="$ARTIFACT_PATH"
if [[ "${BUILDER_OS:-}" == "WINDOWS" ]]; then
  PUSH_PATH="$(cygpath -wa "$ARTIFACT_PATH")"
fi

"$BUTLER" push "$PUSH_PATH" "${ITCH_TARGET}:${ITCH_CHANNEL}"
