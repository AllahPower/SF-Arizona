[CmdletBinding()]
param(
    [string]$Ref = $(if ($env:GITHUB_REF) { $env:GITHUB_REF } else { 'local' }),
    [string]$RunNumber = $env:GITHUB_RUN_NUMBER,
    [string]$RunAttempt = $env:GITHUB_RUN_ATTEMPT,
    [string]$Commit = $env:GITHUB_SHA
)

$ErrorActionPreference = 'Stop'
[xml]$file = Get-Content -LiteralPath "$(Split-Path $PSScriptRoot -Parent)/Version.props"
$base = $file.Project.PropertyGroup.SFBaseVersion
if ($Ref.StartsWith('refs/tags/')) {
    $tag = $Ref.Substring('refs/tags/'.Length)
    if ($tag -notmatch '^v(\d+\.\d+\.\d+)(-[0-9A-Za-z]+(?:[.-][0-9A-Za-z]+)*)?$' -or $Matches[1] -ne $base) {
        throw "Tag $tag must contain base version $base and an optional prerelease suffix."
    }
    $version = $tag.Substring(1)
    $prerelease = $version.Contains('-')
    $publish = $true
}
elseif ($Ref -in @('refs/heads/main', 'refs/heads/experiment/jit-runtime')) {
    if ($RunNumber -notmatch '^\d+$' -or $Commit -notmatch '^[0-9a-fA-F]{7,40}$') { throw 'Preview requires run number and commit SHA.' }
    $version = "$base-preview.$RunNumber.$($Commit.Substring(0, 7))"
    $tag = "v$version"
    $prerelease = $true
    $publish = $true
}
else {
    $version = $base
    $tag = ''
    $prerelease = $false
    $publish = $false
}
# Re-runs update assets for the same immutable ref rather than inventing another release.
@{ version = $version; tag = $tag; prerelease = $prerelease.ToString().ToLowerInvariant(); publish = $publish.ToString().ToLowerInvariant() }.GetEnumerator() |
    Sort-Object Key | ForEach-Object {
        $line = "$($_.Key)=$($_.Value)"
        Write-Output $line
        if ($env:GITHUB_OUTPUT) { Add-Content -LiteralPath $env:GITHUB_OUTPUT -Value $line }
    }
