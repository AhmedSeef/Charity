param([string]$SqlOfflineMedia = '', [string]$SourceServer = '.', [string]$SourceDatabase = 'CharityDb', [switch]$SkipNpmCi)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
. "$root\scripts\deployment\Common.ps1"
$package = Join-Path $root ('devops\releases\Charity-win-x64-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
if (Test-Path -LiteralPath $package) { throw 'Package directory already exists.' }
New-Item -ItemType Directory -Path $package | Out-Null
$app = Join-Path $package 'app'
New-Item -ItemType Directory -Path $app, (Join-Path $package 'database'), (Join-Path $package 'scripts'), (Join-Path $package 'installers') | Out-Null
try {
    Push-Location (Join-Path $root 'charity-app')
    try {
        if (!$SkipNpmCi) {
            & npm.cmd ci
            if ($LASTEXITCODE -ne 0) { throw 'npm ci failed.' }
        }
        & npm.cmd run build -- --configuration production
        if ($LASTEXITCODE -ne 0) { throw 'Angular production build failed.' }
    } finally { Pop-Location }
    & dotnet publish (Join-Path $root 'backend\Charity.API\Charity.API.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o $app
    if ($LASTEXITCODE -ne 0) { throw 'Self-contained publish failed.' }
    # Replace only the newly created package's wwwroot, never the running deployment.
    $webRoot = [IO.Path]::GetFullPath((Join-Path $app 'wwwroot'))
    $boundary = [IO.Path]::GetFullPath($package) + [IO.Path]::DirectorySeparatorChar
    if (!$webRoot.StartsWith($boundary, [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid wwwroot output path.' }
    if (Test-Path -LiteralPath $webRoot) { Remove-Item -LiteralPath $webRoot -Recurse -Force }
    Copy-Item -LiteralPath (Join-Path $root 'charity-app\dist\charity-app\browser') -Destination $webRoot -Recurse
    foreach ($name in @('appsettings.Production.json', 'appsettings.Development.json')) {
        $configFile = Join-Path $app $name
        if (Test-Path -LiteralPath $configFile) { Remove-Item -LiteralPath $configFile }
    }
    Copy-Item -Path "$root\scripts\deployment\*.ps1" -Destination (Join-Path $package 'scripts')
    Copy-Item -LiteralPath "$root\scripts\deployment\README-ar.md" -Destination (Join-Path $package 'README-ar.md')
    $launchers = @{'setup-on-new-machine.bat'='Setup.ps1'; 'start-system.bat'='Start.ps1'; 'stop-system.bat'='Stop.ps1'; 'backup-database.bat'='Backup.ps1'; 'verify-package.bat'='Verify-Package.ps1'}
    foreach ($name in $launchers.Keys) {
        $text = "@echo off`r`npowershell.exe -NoProfile -ExecutionPolicy Bypass -File `"%~dp0scripts\$($launchers[$name])`" %*`r`nif errorlevel 1 (`r`n  echo Operation failed. Read the error above.`r`n  pause`r`n  exit /b 1`r`n)`r`npause`r`n"
        [IO.File]::WriteAllText((Join-Path $package $name), $text, [Text.Encoding]::ASCII)
    }
    if (!$SqlOfflineMedia) { $SqlOfflineMedia = Join-Path $root 'devops\installers\SQL2025' }
    $setup = Join-Path $SqlOfflineMedia 'setup.exe'
    if (!(Test-Path -LiteralPath $setup)) { throw 'Full SQL Server 2025 Express offline media required. Pass -SqlOfflineMedia with the extracted media directory.' }
    $signature = Get-AuthenticodeSignature -FilePath $setup
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notlike '*Microsoft Corporation*') { throw 'Invalid SQL installer signature.' }
    Copy-Item -LiteralPath $SqlOfflineMedia -Destination (Join-Path $package 'installers\SQL2025') -Recurse
    $sourceMachineConfig = Join-Path $package 'installers\SQL2025\SqlServerInstallConfig.ini'
    if (Test-Path -LiteralPath $sourceMachineConfig) { Remove-Item -LiteralPath $sourceMachineConfig }
    $features = Invoke-DatabaseQuery $SourceServer 'SELECT feature_name FROM sys.dm_db_persisted_sku_features' $SourceDatabase
    if ($features.Rows.Count -gt 0) { throw 'Source database uses edition-specific features; review before moving to Express.' }
    Backup-CharityDatabase -Server $SourceServer -Database $SourceDatabase -Destination (Join-Path $package 'database\CharityDb.bak')
    foreach ($required in @('app\Charity.API.exe','app\coreclr.dll','app\hostfxr.dll','app\wwwroot\index.html','installers\SQL2025\setup.exe')) {
        if (!(Test-Path -LiteralPath (Join-Path $package $required))) { throw "Package file missing: $required" }
    }
    $runtime = Get-Content -LiteralPath (Join-Path $app 'Charity.API.runtimeconfig.json') -Raw | ConvertFrom-Json
    if (!$runtime.runtimeOptions.includedFrameworks -or $runtime.runtimeOptions.framework -or $runtime.runtimeOptions.frameworks) { throw 'Publish did not produce a self-contained application.' }
    $manifest = Get-ChildItem -LiteralPath $package -Recurse -File | ForEach-Object {
        [pscustomobject]@{ Path=$_.FullName.Substring($package.Length+1); Bytes=$_.Length; SHA256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
    }
    $manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $package 'manifest.json') -Encoding UTF8
    'Build completed. Verify setup and startup on the target Windows machine.' | Set-Content -LiteralPath (Join-Path $package 'BUILD-COMPLETE.txt')
    Write-Host "Transfer this entire folder: $package" -ForegroundColor Green
} catch {
    $_ | Out-String | Set-Content -LiteralPath (Join-Path $package 'BUILD-FAILED.txt')
    throw
}
