[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$Version,
    [string[]]$RestoreSource = @(),
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (!$Version) {
    [xml]$versionFile = Get-Content -LiteralPath "$root/Version.props"
    $Version = $versionFile.Project.PropertyGroup.SFBaseVersion
}
$runtime = "$root/src/SF.Runtime/SF.Runtime.csproj"
$versionArgument = "-p:Version=$Version"

Push-Location $root
try {
    $projects = @($runtime) + @(Get-ChildItem -LiteralPath "$root/examples" -Filter '*.csproj' -Recurse | ForEach-Object FullName)
    foreach ($project in $projects) {
        if (!$SkipRestore) {
            $restoreArguments = @('restore', $project)
            if ($project -eq $runtime) { $restoreArguments += @('-r', 'win-x86', '-p:SelfContained=false', $versionArgument) }
            foreach ($source in $RestoreSource) { $restoreArguments += @('--source', $source) }
            # Offline caches have no advisory endpoint; normal CI restores retain auditing.
            if ($RestoreSource.Count -gt 0) { $restoreArguments += @('-p:NuGetAudit=false', '-p:EnableRuntimePackDownload=false') }
            & dotnet @restoreArguments
            if ($LASTEXITCODE -ne 0) { throw "Restore failed: $project" }
        }
        $buildArguments = @('build', $project, '-c', $Configuration, '--no-restore')
        if ($project -eq $runtime) { $buildArguments += @('-r', 'win-x86', '-p:SelfContained=false', $versionArgument) }
        & dotnet @buildArguments
        if ($LASTEXITCODE -ne 0) { throw "Build failed: $project" }
    }

    $vswhere = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
    $visualStudio = & $vswhere -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
    if (!$visualStudio) { throw 'Visual Studio C++ x86/x64 build tools were not found.' }
    $dotnetExecutable = (Get-Command dotnet).Source
    $dotnetRoot = Split-Path $dotnetExecutable -Parent
    $packRoot = Join-Path $dotnetRoot 'packs/Microsoft.NETCore.App.Host.win-x86'
    $pack = Get-ChildItem -LiteralPath $packRoot -Directory -ErrorAction SilentlyContinue |
        Where-Object Name -Match '^10\.\d+\.\d+$' |
        Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
    if ($pack) {
        $nativeDirectory = Join-Path $pack.FullName 'runtimes/win-x86/native'
    }
    else {
        # Hosted SDKs may not include x86 nethost headers/libraries. Restore the
        # official host package matching the SDK's bundled .NET runtime version.
        $hostVersion = & dotnet msbuild $runtime -getProperty:BundledNETCoreAppPackageVersion
        if ($LASTEXITCODE -ne 0 -or $hostVersion -notmatch '^10\.\d+\.\d+$') { throw 'Could not resolve .NET host package version.' }
        $hostRestore = @('restore', "$PSScriptRoot/NativeHostPack.proj", "-p:HostPackVersion=$hostVersion")
        foreach ($source in $RestoreSource) { $hostRestore += @('--source', $source) }
        if ($RestoreSource.Count -gt 0) { $hostRestore += '-p:NuGetAudit=false' }
        & dotnet @hostRestore
        if ($LASTEXITCODE -ne 0) { throw 'Could not restore the official win-x86 host package.' }
        $packages = $env:NUGET_PACKAGES
        if (!$packages) { $packages = Join-Path $HOME '.nuget/packages' }
        $nativeDirectory = Join-Path $packages "microsoft.netcore.app.host.win-x86/$hostVersion/runtimes/win-x86/native"
    }

    # A deduplicated child environment avoids MSB6001 with inherited Path/PATH aliases.
    $start = [System.Diagnostics.ProcessStartInfo]::new()
    $start.FileName = Join-Path $visualStudio 'MSBuild/Current/Bin/MSBuild.exe'
    $start.WorkingDirectory = $root
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.Environment.Clear()
    foreach ($entry in [Environment]::GetEnvironmentVariables('Process').GetEnumerator()) {
        $start.Environment[$entry.Key] = $entry.Value
    }
    foreach ($argument in @('src/SF.Native/SF.Native.vcxproj', "/p:Configuration=$Configuration", '/p:Platform=Win32', "/p:NetHostNativeDir=$nativeDirectory", '/v:minimal')) {
        $start.ArgumentList.Add($argument)
    }
    $process = [System.Diagnostics.Process]::Start($start)
    $process.WaitForExit()
    $nativeExitCode = $process.ExitCode
    $process.Dispose()
    if ($nativeExitCode -ne 0) { throw "Native build failed ($nativeExitCode)." }

    & dotnet publish $runtime -c $Configuration -r win-x86 --self-contained false --no-restore $versionArgument
    if ($LASTEXITCODE -ne 0) { throw 'Runtime publish failed.' }
    Write-Output "BUILD PASS: SF.Native, SF.Runtime, SF.Abstractions, SF.Protocol and $($projects.Count - 1) examples; version=$Version"
}
finally {
    Pop-Location
}
