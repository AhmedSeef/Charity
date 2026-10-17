@echo off
title Stop Charity System
echo Stopping Charity.API server...
taskkill /f /im Charity.API.exe 2>nul
if %ERRORLEVEL% EQU 0 (
    echo [OK] Server stopped successfully.
) else (
    echo [INFO] Server was not running.
)
timeout /t 3 >nul
