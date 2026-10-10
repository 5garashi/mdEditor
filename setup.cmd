@echo off
rem File: setup.cmd
rem Description: Runs setup.ps1 by double-click, bypassing the execution policy for this run only
rem Author: 5garashi.com Design Office
rem Created: 2026-10-10
rem License: MIT
rem SPDX-License-Identifier: MIT
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup.ps1"
set "EXIT_CODE=%ERRORLEVEL%"
echo.
if not "%EXIT_CODE%"=="0" echo Setup failed. ^(exit code %EXIT_CODE%^)
pause
exit /b %EXIT_CODE%
