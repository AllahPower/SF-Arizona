[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (!$Version) {
    [xml]$versionFile = Get-Content -LiteralPath "$root/Version.props"
    $Version = $versionFile.Project.PropertyGroup.SFBaseVersion
}
if ($Version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') { throw 'Invalid archive version.' }
$publish = "$root/artifacts/publish/$Configuration/win-x86/SF"
$native = "$root/artifacts/native/$Configuration/win-x86"
foreach ($file in @("$native/SF.asi", "$native/nethost.dll", "$publish/SF.Runtime.dll", "$publish/SF.Abstractions.dll", "$publish/SF.Protocol.dll", "$publish/SF.Runtime.runtimeconfig.json", "$publish/SF.Runtime.deps.json", "$publish/debug-web/wwwroot/index.html")) {
    if (!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "Required deployment file missing: $file" }
}
[xml]$versionFile = Get-Content -LiteralPath "$root/Version.props"
$expectedNumericVersion = "$($versionFile.Project.PropertyGroup.SFBaseVersion).0"
$actual = [Diagnostics.FileVersionInfo]::GetVersionInfo("$publish/SF.Runtime.dll")
if ($actual.FileVersion -ne $expectedNumericVersion -or ($actual.ProductVersion -split '\+')[0] -ne $Version) {
    throw "Published runtime version does not match $Version; build before packaging."
}
$commit = & git -C $root rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Could not resolve build commit.' }
# The base version stays constant between builds, so only the embedded commit exposes a stale publish directory.
$publishedCommit = ($actual.ProductVersion -split '\+', 2)[1]
if ($publishedCommit -ne $commit) {
    throw "Published runtime was built from commit '$publishedCommit', but HEAD is '$commit'; run build.ps1 before packaging."
}
$stagingRoot = [IO.Path]::GetFullPath("$root/artifacts/staging")
$stage = Join-Path $stagingRoot ([guid]::NewGuid().ToString('N'))
$releases = "$root/artifacts/releases"
New-Item -ItemType Directory -Path "$stage/SF", $releases -Force | Out-Null
try {
    Copy-Item -LiteralPath "$native/SF.asi", "$native/nethost.dll" -Destination $stage
    Get-ChildItem -LiteralPath $publish -Force | Copy-Item -Destination "$stage/SF" -Recurse
    Get-ChildItem -LiteralPath $stage -File -Filter '*.pdb' -Recurse | Remove-Item
    Copy-Item -LiteralPath "$root/docs/INSTALL.md" -Destination $stage
    @{ version = $Version; commit = $commit; architecture = 'win-x86'; selfContained = $false } |
        ConvertTo-Json | Set-Content -LiteralPath "$stage/build-info.json" -Encoding utf8
    $archive = "$releases/SF-Arizona-$Version-win-x86.zip"
    Compress-Archive -Path "$stage/*" -DestinationPath $archive -Force
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath "$archive.sha256" -Encoding ascii
    Write-Output "PACKAGE PASS: $archive"
}
finally {
    $resolved = [IO.Path]::GetFullPath($stage)
    if (!$resolved.StartsWith($stagingRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Staging cleanup target is outside artifacts/staging.'
    }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
