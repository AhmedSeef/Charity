# Charity Federation - Local Deploy Script
$ErrorActionPreference = "Stop"

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " [1/3] Building Angular 21 Frontend (Production)..." -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Cyan

Set-Location -Path "D:\Charity\charity-app"
npm run build

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " [2/3] Copying Frontend to Backend wwwroot..." -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Cyan

$wwwroot = "D:\Charity\backend\Charity.API\wwwroot"
if (!(Test-Path $wwwroot)) {
    New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
}
Copy-Item -Path "D:\Charity\charity-app\dist\charity-app\browser\*" -Destination $wwwroot -Recurse -Force

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Stopping running Charity.API processes if any..." -ForegroundColor DarkYellow
Get-Process Charity.API -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

$deployDir = "D:\Charity\deploy"
dotnet publish "D:\Charity\backend\Charity.API\Charity.API.csproj" -c Release -o $deployDir

Write-Host ""
Write-Host "========================================================" -ForegroundColor Green
Write-Host "  Deployment Completed Successfully!" -ForegroundColor Green
Write-Host "  Deploy Folder:  D:\Charity\deploy" -ForegroundColor White
Write-Host "  Start Script:   D:\Charity\start-system.bat" -ForegroundColor Cyan
Write-Host "  Local URL:      http://localhost:5072" -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Green
