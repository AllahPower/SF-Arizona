[CmdletBinding()]
param([string]$Version)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
[xml]$versions = Get-Content -LiteralPath "$root/Version.props"
$base = $versions.Project.PropertyGroup.SFBaseVersion
if (!$Version) { $Version = $base }
if ($Version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z]+(?:[.-][0-9A-Za-z]+)*)?$') {
    throw "Invalid package version: $Version"
}
if (($Version -split '-', 2)[0] -ne $base) { throw "Package version must use base $base" }
$output = "$root/artifacts/packages/$Version"
foreach ($project in 'SF.Abstractions', 'SF.Protocol') {
    $projectPath = "$root/src/$project/$project.csproj"
    $packageId = & dotnet msbuild $projectPath "-p:Version=$Version" -getProperty:PackageId
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($packageId)) {
        throw "Could not resolve PackageId for $project"
    }
    & dotnet pack $projectPath -c Release "-p:Version=$Version" --output $output
    if ($LASTEXITCODE -ne 0) { throw "Packing $project failed" }
    foreach ($extension in 'nupkg', 'snupkg') {
        if (!(Test-Path -LiteralPath "$output/$packageId.$Version.$extension")) {
            throw "Missing package: $packageId.$Version.$extension"
        }
    }
}
Write-Output "Packages: $output"
