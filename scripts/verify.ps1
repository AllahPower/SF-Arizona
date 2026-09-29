[CmdletBinding()]
param([string]$Archive)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
function Assert-True([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
}

$expectedProjects = @('SF.Abstractions', 'SF.Runtime', 'SF.Native')
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
$runtimeReferences = @($runtime.Project.ItemGroup.ProjectReference | Where-Object { $_ })
Assert-True ($runtimeReferences.Count -eq 1 -and $runtimeReferences[0].Include -eq '..\SF.Abstractions\SF.Abstractions.csproj') 'Runtime must reference only Abstractions.'
Assert-True (@($abstractions.Project.ItemGroup.ProjectReference | Where-Object { $_ }).Count -eq 0) 'Abstractions must not reference implementations.'
foreach ($source in Get-ChildItem -LiteralPath "$root/src/SF.Abstractions" -Filter '*.cs' -Recurse) {
    Assert-True (!(Select-String -LiteralPath $source.FullName -Pattern '\bSFSharp\.Runtime\b|\[(?:DllImport|LibraryImport)\b' -Quiet)) "Implementation dependency in contracts: $($source.Name)"
}
$native = Get-Content -LiteralPath "$root/src/SF.Native/src/runtime/hostfxr_bootstrap.cpp" -Raw
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
        foreach ($file in @('SF.asi', 'nethost.dll', 'SF/SF.Runtime.dll', 'SF/SF.Abstractions.dll', 'SF/SF.Runtime.runtimeconfig.json', 'SF/SF.Runtime.deps.json', 'SF/debug-web/wwwroot/index.html', 'INSTALL.md', 'build-info.json')) {
            Assert-True ($file -in $entries) "Archive is missing $file"
        }
        foreach ($file in $entries) {
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
