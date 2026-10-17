@echo off
title Charity Federation - Build & Deploy
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0deploy.ps1"
if %ERRORLEVEL% EQU 0 (
    echo.
    echo [OK] Done!
) else (
    echo.
    echo [ERROR] Build or deployment failed!
)
pause
