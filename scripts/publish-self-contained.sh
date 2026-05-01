#!/usr/bin/env bash
set -euo pipefail

# Publish LotofacilMcp.Server as self-contained executables (no SDK required to run).
# Outputs go to artifacts/publish/<rid>/

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/src/LotofacilMcp.Server/LotofacilMcp.Server.csproj"
OUT_ROOT="$ROOT_DIR/artifacts/publish"

CONFIGURATION="${CONFIGURATION:-Release}"

# v1 support matrix (ADR 0024):
# - Required: win-x64
# - Planned: linux-x64, osx-x64, osx-arm64
RIDS=(
  "win-x64"
)

for rid in "${RIDS[@]}"; do
  out="$OUT_ROOT/$rid"
  echo "Publishing $rid -> $out"
  dotnet publish "$PROJECT" \
    -c "$CONFIGURATION" \
    -r "$rid" \
    --self-contained true \
    -o "$out" \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true
done

echo "Done. Output folder: $OUT_ROOT"
