[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [switch]$NoBuild,
    [string[]]$RestoreSource = @()
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (!$NoBuild) {
    $arguments = @('build', "$root/examples/DebugWeb/DebugWeb.Example.csproj", '-c', $Configuration)
    foreach ($source in $RestoreSource) { $arguments += @('--source', $source) }
    if ($RestoreSource.Count -gt 0) { $arguments += @('-p:NuGetAudit=false', '-p:EnableRuntimePackDownload=false') }
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) { throw 'DebugWeb build failed.' }
}
$output = "$root/artifacts/examples/$Configuration/debug-web"
foreach ($name in 'manifest.json', 'SF.Example.DebugWeb.dll', 'SF.Example.DebugWeb.deps.json', 'wwwroot/index.html') {
    if (!(Test-Path -LiteralPath "$output/$name" -PathType Leaf)) { throw "DebugWeb deployment file is missing: $name" }
}
$manifest = Get-Content -LiteralPath "$output/manifest.json" -Raw | ConvertFrom-Json
$version = $manifest.version
if ($version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') { throw 'Invalid plugin version.' }
$commit = & git -C $root rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Could not resolve plugin source commit.' }
$productVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo("$output/SF.Example.DebugWeb.dll").ProductVersion
if (($productVersion -split '\+')[0] -ne $version -or ($productVersion -split '\+', 2)[1] -ne $commit) {
    throw 'DebugWeb output does not match its manifest version and current commit; rebuild without -NoBuild.'
}
$files = @(Get-ChildItem -LiteralPath $output -File -Recurse | Where-Object Extension -ne '.pdb')
foreach ($file in $files) {
    if ($file.Name -in 'SF.Runtime.dll', 'SF.Abstractions.dll', 'SF.Protocol.dll') { throw "Host assembly must not ship with DebugWeb: $($file.Name)" }
}
$releases = "$root/artifacts/releases"
New-Item -ItemType Directory -Path $releases -Force | Out-Null
$archive = "$releases/SF-DebugWeb-$version-win-x86.zip"
if (Test-Path -LiteralPath $archive) { Remove-Item -LiteralPath $archive -Force }
$zip = [IO.Compression.ZipFile]::Open($archive, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in $files) {
        $relative = [IO.Path]::GetRelativePath($output, $file.FullName).Replace('\', '/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, "SF/modules/debug-web/$relative") | Out-Null
    }
}
finally { $zip.Dispose() }
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath "$archive.sha256" -Encoding ascii
$verify = [IO.Compression.ZipFile]::OpenRead($archive)
try {
    foreach ($name in 'manifest.json', 'SF.Example.DebugWeb.dll', 'SF.Example.DebugWeb.deps.json', 'wwwroot/index.html') {
        if (!$verify.GetEntry("SF/modules/debug-web/$name")) { throw "DebugWeb archive is missing $name" }
    }
    if (@($verify.Entries | Where-Object { $_.FullName -match 'node_modules|\.(pdb|ts|tsx)$' }).Count) { throw 'Source or build artifacts leaked into DebugWeb archive.' }
}
finally { $verify.Dispose() }
Write-Output "DEBUGWEB PACKAGE PASS: $archive ($($files.Count) files, SHA256 $hash)"
