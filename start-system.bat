@echo off
title Charity Federation System (Local Server)
echo ========================================================
echo   Starting Charity Federation System...
echo   Local Address:   http://localhost:5072
echo   Network Address: http://0.0.0.0:5072
echo ========================================================

cd /d "D:\Charity\deploy"

:: Launch default browser after 2 seconds
start "" cmd /c "timeout /t 2 /nobreak >nul && start http://localhost:5072"

:: Run the self-hosted ASP.NET Core server
Charity.API.exe
pause
