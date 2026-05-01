# ADR 0024 (v1) ZIP packaging for self-contained MCP server (STDIO + HTTP).
#
# Goals:
# - Generate a versioned ZIP per RID: lotofacil-ia-mcp-stdio-<rid>-vX.Y.Z.zip
# - ZIP content: published runtime files + a short README inside the ZIP
# - Dataset policy (ADR 0022): do NOT include any dataset by default; optional sample is opt-in.
#
# Usage examples:
#   pwsh ./scripts/package-zip-v1.ps1 -Version 1.2.3
#   pwsh ./scripts/package-zip-v1.ps1 -Version 1.2.3 -Rid win-x64
#   pwsh ./scripts/package-zip-v1.ps1 -Version 1.2.3 -IncludeSampleDataset

param(
  [Parameter(Mandatory = $true)]
  [string]$Version,

  [string]$Rid = "win-x64",

  [string]$Configuration = $(if ($env:CONFIGURATION) { $env:CONFIGURATION } else { "Release" }),

  [switch]$SkipPublish,

  [switch]$IncludeSampleDataset
)

$ErrorActionPreference = "Stop"

function New-CleanDirectory([string]$Path) {
  if (Test-Path $Path) { Remove-Item -Recurse -Force $Path }
  New-Item -ItemType Directory -Path $Path | Out-Null
}

function Get-RootDir() {
  return (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

function Get-ZipName([string]$rid, [string]$version) {
  $v = $version.Trim()
  if (-not $v.StartsWith("v")) { $v = "v$v" }
  return "lotofacil-ia-mcp-stdio-$rid-$v.zip"
}

function Write-ReadmeTxt([string]$Path, [string]$rid, [string]$version) {
  $content = @"
Lotofacil-IA MCP Server (ZIP v1) — $rid — v$version

This ZIP contains a self-contained executable for running the Lotofacil-IA MCP server
without source code or the .NET SDK.

Modes:
  - STDIO (MCP hosts like Cursor): run with --mcp-stdio
  - HTTP (when supported by your host): run with no flags

Required environment (no hidden defaults):
  - Dataset__DrawsSourceUri  (ADR 0022)  [REQUIRED]
    Examples:
      file:///C:/dados/lotofacil/draws.json
      C:\dados\lotofacil.csv
      https://example.com/lotofacil.json

Cursor (STDIO) configuration example:
{
  "mcpServers": {
    "lotofacil-ia": {
      "command": "C:\\LotofacilIA\\LotofacilMcp.Server.exe",
      "args": ["--mcp-stdio"],
      "env": {
        "Dataset__DrawsSourceUri": "file:///C:/dados/lotofacil/draws.json"
      }
    }
  }
}

Manual HTTP example (PowerShell):
  $env:Dataset__DrawsSourceUri = "file:///C:/dados/lotofacil/draws.json"
  $env:ASPNETCORE_URLS = "http://127.0.0.1:5000"
  & ".\LotofacilMcp.Server.exe"

Dataset policy:
  - No dataset is bundled by default (ADR 0024 + ADR 0022).
  - If a sample dataset is included, it's opt-in and not auto-selected.

Docs:
  - ADR 0024 (ZIP v1): docs/adrs/0024-distribuicao-zip-mcp-stdio-http-sem-codigo-fonte-v1.md
  - ADR 0022 (dataset): docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md
  - Repo README: https://github.com/<your-org>/<your-repo> (or open README.md in the source repo)
"@

  Set-Content -Path $Path -Value $content -Encoding UTF8
}

$RootDir = Get-RootDir
$Project = Join-Path $RootDir "src\LotofacilMcp.Server\LotofacilMcp.Server.csproj"

$PublishRoot = Join-Path $RootDir "artifacts\publish"
$PublishDir = Join-Path $PublishRoot $Rid

$DistRoot = Join-Path $RootDir "artifacts\dist"
$ZipName = Get-ZipName -rid $Rid -version $Version
$ZipPath = Join-Path $DistRoot $ZipName

$StageRoot = Join-Path $RootDir "artifacts\stage"
$StageDir = Join-Path $StageRoot $Rid

if (-not $SkipPublish) {
  # Avoid locked/partially-written single-file outputs from previous runs.
  if (Test-Path $PublishDir) {
    try {
      Remove-Item -Recurse -Force $PublishDir
    } catch {
      throw "Failed to clean publish dir '$PublishDir'. Close any running LotofacilMcp.Server processes and retry. $($_.Exception.Message)"
    }
  }

  Write-Host "Publishing $Rid -> $PublishDir"
  dotnet publish $Project `
    -c $Configuration `
    -r $Rid `
    --self-contained true `
    -o $PublishDir `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true

  if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
  }
}

if (-not (Test-Path $PublishDir)) {
  throw "Publish output not found: $PublishDir. Run publish first or omit -SkipPublish."
}

New-CleanDirectory $StageDir

Write-Host "Staging runtime files -> $StageDir"
Copy-Item -Path (Join-Path $PublishDir "*") -Destination $StageDir -Recurse -Force

$ReadmePath = Join-Path $StageDir "README.txt"
Write-ReadmeTxt -Path $ReadmePath -rid $Rid -version $Version

if ($IncludeSampleDataset) {
  $fixture = Join-Path $RootDir "tests\fixtures\synthetic_min_window.json"
  if (-not (Test-Path $fixture)) {
    throw "Sample fixture not found: $fixture"
  }

  $sampleDir = Join-Path $StageDir "sample"
  New-Item -ItemType Directory -Path $sampleDir | Out-Null
  Copy-Item -Path $fixture -Destination (Join-Path $sampleDir "draws.sample.json") -Force
}

New-Item -ItemType Directory -Path $DistRoot -Force | Out-Null
if (Test-Path $ZipPath) { Remove-Item -Force $ZipPath }

Write-Host "Creating ZIP -> $ZipPath"
Compress-Archive -Path (Join-Path $StageDir "*") -DestinationPath $ZipPath

Write-Host "Done."
Write-Host "ZIP: $ZipPath"
