[CmdletBinding()]
param([string]$Archive, [string]$DebugWebArchive)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
function Assert-True([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
}

$expectedProjects = @('SF.Abstractions', 'SF.Protocol', 'SF.Runtime', 'SF.Native')
$tracked = & git -C $root ls-files --cached --others --exclude-standard src
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect source files.' }
foreach ($path in $tracked) {
    if (!(Test-Path -LiteralPath "$root/$path")) { continue }
    $project = ($path -split '/')[1]
    Assert-True ($project -in $expectedProjects) "Unexpected source-root item: $path"
    Assert-True ($path -notmatch '/(bin|obj|\.vs)/|\.(user|pdb|log|bak)$') "Generated source artifact: $path"
}
[xml]$runtime = Get-Content -LiteralPath "$root/src/SF.Runtime/SF.Runtime.csproj"
[xml]$abstractions = Get-Content -LiteralPath "$root/src/SF.Abstractions/SF.Abstractions.csproj"
[xml]$protocol = Get-Content -LiteralPath "$root/src/SF.Protocol/SF.Protocol.csproj"
$runtimeReferences = @($runtime.Project.ItemGroup.ProjectReference | Where-Object { $_ } | ForEach-Object Include | Sort-Object)
Assert-True (($runtimeReferences -join ';') -eq '..\SF.Abstractions\SF.Abstractions.csproj;..\SF.Protocol\SF.Protocol.csproj') 'Runtime must reference only Abstractions and Protocol.'
$protocolReferences = @($protocol.Project.ItemGroup.ProjectReference | Where-Object { $_ } | ForEach-Object Include)
Assert-True ($protocolReferences.Count -eq 1 -and $protocolReferences[0] -eq '..\SF.Abstractions\SF.Abstractions.csproj') 'Protocol must reference only Abstractions.'
Assert-True (@($abstractions.Project.ItemGroup.ProjectReference | Where-Object { $_ }).Count -eq 0) 'Abstractions must not reference implementations.'
[xml]$debugWeb = Get-Content -LiteralPath "$root/examples/DebugWeb/DebugWeb.Example.csproj"
$debugReferences = @($debugWeb.Project.ItemGroup.ProjectReference | Where-Object { $_ } | ForEach-Object Include)
Assert-True ($debugReferences.Count -eq 2 -and '../../src/SF.Abstractions/SF.Abstractions.csproj' -in $debugReferences -and '../../src/SF.Protocol/SF.Protocol.csproj' -in $debugReferences) 'DebugWeb must reference only host contracts and Protocol.'
foreach ($source in Get-ChildItem -LiteralPath "$root/examples/DebugWeb" -Filter '*.cs' -Recurse | Where-Object { $_.FullName -notmatch '[\\/]node_modules[\\/]' }) {
    Assert-True (!(Select-String -LiteralPath $source.FullName -Pattern '\bSFSharp\.Runtime\b|\[(?:DllImport|LibraryImport)\b|delegate\* unmanaged' -Quiet)) "Runtime/native dependency in DebugWeb: $($source.Name)"
}
Assert-True (!(Test-Path -LiteralPath "$root/src/SF.Runtime/Diagnostics/DebugWeb/DebugModule.cs")) 'DebugWeb must be an external plugin.'
$clientSpecificSymbols = '\bSFSharp\.(?:Protocol|Abstractions)\.Arizona\b|\b(?:ISFArizonaPackets|ArizonaPacket220Id|ArizonaPacket221Id|AZVoiceMessageId|SampfuncsInfo|CDialogCloseSampfuncsHook)\b|AZVoice\.asi|sampfuncs\.asi'
foreach ($source in Get-ChildItem -LiteralPath "$root/src" -Filter '*.cs' -Recurse | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }) {
    Assert-True (!(Select-String -LiteralPath $source.FullName -Pattern $clientSpecificSymbols -Quiet)) "Client-specific integration in the SA-MP core: $($source.FullName)"
}
Assert-True (!(Test-Path -LiteralPath "$root/examples/CefEmitor/CefEmitor.Example.csproj")) 'Client-specific CEF samples must not ship with the core.'
foreach ($project in 'SF.Abstractions', 'SF.Protocol') {
    foreach ($source in Get-ChildItem -LiteralPath "$root/src/$project" -Filter '*.cs' -Recurse | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }) {
        Assert-True (!(Select-String -LiteralPath $source.FullName -Pattern '\bSFSharp\.Runtime\b|\[(?:DllImport|LibraryImport)\b|delegate\* unmanaged' -Quiet)) "Native or implementation dependency in $project`: $($source.Name)"
    }
}
$native = Get-Content -LiteralPath "$root/src/SF.Native/src/runtime/hostfxr_bootstrap.cpp" -Raw
foreach ($folder in 'src/SF.Runtime/Gta', 'src/SF.Runtime/Interop/Gta', 'src/SF.Abstractions/Api/Gta') {
    foreach ($source in Get-ChildItem -LiteralPath "$root/$folder" -Filter '*.cs' -Recurse) {
        Assert-True (!(Select-String -LiteralPath $source.FullName -Pattern '\b(?:SampOffsets|CNetGame|CPlayerPool|CLocalPlayer|SFPlayers|SFRpc)\b|using SFSharp\..*\.Samp\b' -Quiet)) "SA-MP implementation dependency in GTA code: $($source.FullName)"
    }
}
foreach ($project in $runtime, $abstractions, $protocol) {
    Assert-True ('14' -in @($project.Project.PropertyGroup.LangVersion)) 'Host projects must target stable C# 14.'
}
Assert-True ($native.Contains('SFSharp.Runtime.Bootstrap.SFBootstrap, SF.Runtime')) 'Managed bootstrap identity changed.'
$solution = Get-Content -LiteralPath "$root/SF-Arizona.sln" -Raw
foreach ($project in $expectedProjects) {
    Assert-True ($solution.Contains("src\$project\$project.")) "Solution is missing $project."
}
Assert-True (!(Test-Path -LiteralPath "$root/src/SF.slnx")) 'Duplicate obsolete solution still exists.'
$ignore = Get-Content -LiteralPath "$root/.gitignore"
Assert-True ('AGENTS.md' -in $ignore -and 'CLAUDE.md' -in $ignore) 'Local agent instructions must stay ignored.'

[xml]$versionFile = Get-Content -LiteralPath "$root/Version.props"
$base = $versionFile.Project.PropertyGroup.SFBaseVersion
Assert-True ($base -match '^\d+\.\d+\.\d+$') 'Base version must be numeric SemVer.'
$previousOutput = $env:GITHUB_OUTPUT
try {
    $env:GITHUB_OUTPUT = $null
    $stable = @(& "$PSScriptRoot/release-version.ps1" -Ref "refs/tags/v$base")
    Assert-True ('prerelease=false' -in $stable -and 'publish=true' -in $stable -and "version=$base" -in $stable) 'Stable version routing failed.'
    $rc = @(& "$PSScriptRoot/release-version.ps1" -Ref "refs/tags/v$base-rc.1")
    Assert-True ('prerelease=true' -in $rc -and "version=$base-rc.1" -in $rc) 'Prerelease version routing failed.'
    $preview = @(& "$PSScriptRoot/release-version.ps1" -Ref 'refs/heads/experiment/jit-runtime' -RunNumber 42 -Commit 'abcdef0123456789')
    Assert-True ("version=$base-preview.42.abcdef0" -in $preview -and 'prerelease=true' -in $preview) 'Preview version routing failed.'
    $retry = @(& "$PSScriptRoot/release-version.ps1" -Ref 'refs/heads/experiment/jit-runtime' -RunNumber 42 -RunAttempt 2 -Commit 'abcdef0123456789')
    Assert-True (($preview -join "`n") -eq ($retry -join "`n")) 'Re-run changed the release identity.'
    $pr = @(& "$PSScriptRoot/release-version.ps1" -Ref 'refs/pull/123/merge')
    Assert-True ('publish=false' -in $pr) 'Pull requests must not publish releases.'
    $main = @(& "$PSScriptRoot/release-version.ps1" -Ref 'refs/heads/main' -RunNumber 43 -Commit '123456789abcdef0')
    Assert-True ("version=$base-preview.43.1234567" -in $main -and 'prerelease=true' -in $main -and 'publish=true' -in $main) 'Main preview routing failed.'
    $mainRetry = @(& "$PSScriptRoot/release-version.ps1" -Ref 'refs/heads/main' -RunNumber 43 -RunAttempt 2 -Commit '123456789abcdef0')
    Assert-True (($main -join "`n") -eq ($mainRetry -join "`n")) 'Main re-run changed the preview identity.'
    $rejected = $false
    try { & "$PSScriptRoot/release-version.ps1" -Ref 'refs/tags/v999.0.0' | Out-Null } catch { $rejected = $true }
    Assert-True $rejected 'Version mismatch was accepted.'
}
finally { $env:GITHUB_OUTPUT = $previousOutput }
Write-Output 'VERIFY PASS: project boundaries, source layout, bootstrap identity, stable/rc/preview/PR/mismatch/re-run routing'

if ($Archive) {
    $archivePath = [IO.Path]::GetFullPath((Join-Path $root $Archive))
    $zip = [IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $entries = @($zip.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
        foreach ($file in @('SF.asi', 'nethost.dll', 'SF/SF.Runtime.dll', 'SF/SF.Abstractions.dll', 'SF/SF.Protocol.dll', 'SF/SF.Runtime.runtimeconfig.json', 'SF/SF.Runtime.deps.json', 'INSTALL.md', 'build-info.json')) {
            Assert-True ($file -in $entries) "Archive is missing $file"
        }
        foreach ($file in $entries) {
            Assert-True ($file -notmatch '^SF/(debug-web|modules/debug-web)/') "DebugWeb must be distributed as a separate plugin archive: $file"
            Assert-True ($file -notmatch '(^|/)(bin|obj|\.vs|examples)/|\.(pdb|user|log|bak)$|(^|/)sf-host\.json$') "Unexpected deployment entry: $file"
        }
        $reader = [IO.StreamReader]::new($zip.GetEntry('SF/SF.Runtime.runtimeconfig.json').Open())
        try { $config = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
        $frameworks = @($config.runtimeOptions.frameworks.name)
        Assert-True ('Microsoft.NETCore.App' -in $frameworks -and 'Microsoft.AspNetCore.App' -in $frameworks) 'Expected framework-dependent runtime configuration.'
        $expectedHash = ((Get-Content -LiteralPath "$archivePath.sha256" -Raw).Trim() -split '\s+')[0]
        $actualHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
        Assert-True ($expectedHash -eq $actualHash) 'Archive checksum mismatch.'
        Write-Output "ARCHIVE PASS: $($entries.Count) entries; framework-dependent win-x86 layout and SHA256 verified"
    }
    finally { $zip.Dispose() }
}

if ($DebugWebArchive) {
    $archivePath = [IO.Path]::GetFullPath((Join-Path $root $DebugWebArchive))
    $zip = [IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $entries = @($zip.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
        foreach ($name in 'manifest.json', 'SF.Example.DebugWeb.dll', 'SF.Example.DebugWeb.deps.json', 'wwwroot/index.html') {
            Assert-True ("SF/modules/debug-web/$name" -in $entries) "DebugWeb archive is missing $name"
        }
        foreach ($entry in $entries) {
            Assert-True ($entry.StartsWith('SF/modules/debug-web/') -and $entry -notmatch 'node_modules|\.(pdb|tsx?|log)$|SF\.(Runtime|Abstractions|Protocol)\.dll$') "Unexpected DebugWeb archive file: $entry"
        }
        $expectedHash = ((Get-Content -LiteralPath "$archivePath.sha256" -Raw).Trim() -split '\s+')[0]
        Assert-True ($expectedHash -eq (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()) 'DebugWeb checksum mismatch.'
        Write-Output "DEBUGWEB ARCHIVE PASS: $($entries.Count) entries; plugin isolation and SHA256 verified"
    }
    finally { $zip.Dispose() }
}
