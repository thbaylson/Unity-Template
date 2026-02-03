#!/usr/bin/env bash
set -euo pipefail

# UBA artifact path (Unity support references UNITY_PLAYER_PATH in post-build scripts)
PLAYER_PATH="${UNITY_PLAYER_PATH:-}"
if [[ -z "$PLAYER_PATH" ]]; then
  echo "UNITY_PLAYER_PATH is empty; cannot locate build artifact."
  exit 1
fi

# If the artifact is a directory, ensure it contains index.html (itch HTML5 entry point)
if [[ -d "$PLAYER_PATH" ]]; then
  if [[ ! -f "$PLAYER_PATH/index.html" ]]; then
    echo "No index.html found at: $PLAYER_PATH"
    exit 1
  fi
fi

# Download butler (automation-friendly, stable URL)
curl -L -o butler.zip "https://broth.itch.zone/butler/linux-amd64/LATEST/archive/default"
unzip -o butler.zip -d butler_bin
chmod +x butler_bin/butler*

# Push (butler accepts a directory or a .zip)
TARGET="${ITCH_TARGET:?Missing ITCH_TARGET}"
CHANNEL="${ITCH_CHANNEL:?Missing ITCH_CHANNEL}"

butler_bin/butler push "$PLAYER_PATH" "${TARGET}:${CHANNEL}"