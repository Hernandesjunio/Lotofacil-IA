#!/usr/bin/env bash
set -euo pipefail

# ADR 0024 (v1) ZIP packaging for self-contained MCP server (STDIO + HTTP).
#
# Goals:
# - Generate a versioned ZIP per RID: lotofacil-ia-mcp-stdio-<rid>-vX.Y.Z.zip
# - ZIP content: published runtime files + a short README inside the ZIP
# - Dataset policy (ADR 0022): do NOT include any dataset by default; optional sample is opt-in.
#
# Usage examples:
#   ./scripts/package-zip-v1.sh --version 1.2.3
#   ./scripts/package-zip-v1.sh --version 1.2.3 --rid win-x64
#   ./scripts/package-zip-v1.sh --version 1.2.3 --include-sample-dataset

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/src/LotofacilMcp.Server/LotofacilMcp.Server.csproj"

CONFIGURATION="${CONFIGURATION:-Release}"
RID="win-x64"
VERSION=""
SKIP_PUBLISH="false"
INCLUDE_SAMPLE_DATASET="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      VERSION="${2:-}"; shift 2;;
    --rid)
      RID="${2:-}"; shift 2;;
    --skip-publish)
      SKIP_PUBLISH="true"; shift 1;;
    --include-sample-dataset)
      INCLUDE_SAMPLE_DATASET="true"; shift 1;;
    *)
      echo "Unknown argument: $1" >&2
      exit 2;;
  esac
done

if [[ -z "$VERSION" ]]; then
  echo "--version is required (e.g. --version 1.2.3)" >&2
  exit 2
fi

ZIP_VERSION="$VERSION"
if [[ "$ZIP_VERSION" != v* ]]; then
  ZIP_VERSION="v$ZIP_VERSION"
fi

PUBLISH_DIR="$ROOT_DIR/artifacts/publish/$RID"
DIST_ROOT="$ROOT_DIR/artifacts/dist"
STAGE_DIR="$ROOT_DIR/artifacts/stage/$RID"

ZIP_NAME="lotofacil-ia-mcp-stdio-$RID-$ZIP_VERSION.zip"
ZIP_PATH="$DIST_ROOT/$ZIP_NAME"

if [[ "$SKIP_PUBLISH" != "true" ]]; then
  echo "Publishing $RID -> $PUBLISH_DIR"
  dotnet publish "$PROJECT" \
    -c "$CONFIGURATION" \
    -r "$RID" \
    --self-contained true \
    -o "$PUBLISH_DIR" \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true
fi

if [[ ! -d "$PUBLISH_DIR" ]]; then
  echo "Publish output not found: $PUBLISH_DIR" >&2
  exit 1
fi

rm -rf "$STAGE_DIR"
mkdir -p "$STAGE_DIR"

echo "Staging runtime files -> $STAGE_DIR"
cp -R "$PUBLISH_DIR"/. "$STAGE_DIR"/

cat > "$STAGE_DIR/README.txt" <<EOF
Lotofacil-IA MCP Server (ZIP v1) — $RID — v$VERSION

This ZIP contains a self-contained executable for running the Lotofacil-IA MCP server
without source code or the .NET SDK.

Modes:
  - STDIO (MCP hosts like Cursor): run with --mcp-stdio
  - HTTP (when supported by your host): run with no flags

Required environment (no hidden defaults):
  - Dataset__DrawsSourceUri  (ADR 0022)  [REQUIRED]
    Examples:
      file:///C:/dados/lotofacil/draws.json
      C:\\dados\\lotofacil.csv
      https://example.com/lotofacil.json

Manual HTTP example:
  export Dataset__DrawsSourceUri="file:///C:/dados/lotofacil/draws.json"
  export ASPNETCORE_URLS="http://127.0.0.1:5000"
  ./LotofacilMcp.Server

Dataset policy:
  - No dataset is bundled by default (ADR 0024 + ADR 0022).
  - If a sample dataset is included, it's opt-in and not auto-selected.

Docs:
  - ADR 0024 (ZIP v1): docs/adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md
  - ADR 0022 (dataset): docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md
EOF

if [[ "$INCLUDE_SAMPLE_DATASET" == "true" ]]; then
  FIXTURE="$ROOT_DIR/tests/fixtures/synthetic_min_window.json"
  if [[ ! -f "$FIXTURE" ]]; then
    echo "Sample fixture not found: $FIXTURE" >&2
    exit 1
  fi
  mkdir -p "$STAGE_DIR/sample"
  cp "$FIXTURE" "$STAGE_DIR/sample/draws.sample.json"
fi

mkdir -p "$DIST_ROOT"
rm -f "$ZIP_PATH"

echo "Creating ZIP -> $ZIP_PATH"
python3 - <<PY
import os, zipfile
stage = os.environ["STAGE_DIR"]
zip_path = os.environ["ZIP_PATH"]
with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as z:
    for root, dirs, files in os.walk(stage):
        for f in files:
            p = os.path.join(root, f)
            arc = os.path.relpath(p, stage)
            z.write(p, arcname=arc)
print(zip_path)
PY

echo "Done."
echo "ZIP: $ZIP_PATH"
