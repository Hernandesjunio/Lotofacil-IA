$ErrorActionPreference = "Stop"

# Publish LotofacilMcp.Server as self-contained executables (no SDK required to run).
# Outputs go to artifacts/publish/<rid>\

$RootDir = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Project = Join-Path $RootDir "src\LotofacilMcp.Server\LotofacilMcp.Server.csproj"
$OutRoot = Join-Path $RootDir "artifacts\publish"

$Configuration = if ($env:CONFIGURATION) { $env:CONFIGURATION } else { "Release" }

# v1 support matrix (ADR 0024):
# - Required: win-x64
# - Planned: linux-x64, osx-x64, osx-arm64
$Rids = @(
  "win-x64"
)

foreach ($rid in $Rids) {
  $out = Join-Path $OutRoot $rid
  Write-Host "Publishing $rid -> $out"
  dotnet publish $Project `
    -c $Configuration `
    -r $rid `
    --self-contained true `
    -o $out `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true
}

Write-Host "Done. Output folder: $OutRoot"

