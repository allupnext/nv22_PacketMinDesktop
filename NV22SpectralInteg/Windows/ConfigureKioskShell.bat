@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

:: ======================================================
:: === CHECK FOR ADMIN PRIVILEGES ===
:: ======================================================
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo Requesting Administrator privileges to run setup...
    powershell -Command "Start-Process -FilePath '%~dpnx0' -Verb RunAs"
    exit /b
)

cls
echo ======================================================
echo === Step 1: Finding the KioskUser SID
echo ======================================================

set "KioskUserSID="
for /f "usebackq tokens=*" %%a in (`powershell -NoProfile -ExecutionPolicy Bypass -Command "(Get-LocalUser -Name 'KioskUser').SID.Value"`) do (
    set "KioskUserSID=%%a"
)

if not defined KioskUserSID (
    echo [ERROR] Could not retrieve KioskUser SID.
    echo Make sure the user "KioskUser" exists before running this script.
    pause
    exit /b
)

echo KioskUser SID found: %KioskUserSID%
echo.

:: ======================================================
:: === CONFIGURABLE APP PATH ===
:: ======================================================
set "AppFolder=C:\Program Files (x86)\PocketMint"
set "AppExe=%AppFolder%\NV22SpectralInteg.exe"

if not exist "%AppExe%" (
    echo [WARNING] Application not found at "%AppExe%"
    echo Please verify that your application is installed correctly.
    pause
)

echo ======================================================
echo === Step 2: Setting Registry Key for Kiosk Shell
echo ======================================================

set "RegPath=HKEY_USERS\%KioskUserSID%\Software\Microsoft\Windows NT\CurrentVersion\Winlogon"

echo Creating or updating Shell value under:
echo %RegPath%
echo.

:: Add or update the Shell registry value
powershell -Command ^
    "New-Item -Path 'Registry::%RegPath%' -Force | Out-Null; " ^
    "Set-ItemProperty -Path 'Registry::%RegPath%' -Name 'Shell' -Value '%AppExe%' -Force"

if %errorlevel% neq 0 (
    echo [ERROR] Failed to set the registry key for Shell.
    pause
    exit /b
)
echo Shell successfully set to: %AppExe%
echo.

:: ======================================================
:: === Step 3: Granting Folder Permissions ===
:: ======================================================

echo Granting Full Control permissions for KioskUser on:
echo %AppFolder%
echo.

icacls "%AppFolder%" /grant:r "KioskUser":F /T /C >nul

if %errorlevel% neq 0 (
    echo [WARNING] Failed to set permissions. Continuing anyway...
) else (
    echo Permissions granted successfully.
)
echo.

:: ======================================================
:: === Step 4: Finalizing Setup ===
:: ======================================================
echo Configuration complete!
echo When KioskUser logs in, Windows will launch:
echo %AppExe%
echo as the only shell application.
echo.
pause

echo Shutting down system now...
shutdown /s /t 0

ENDLOCAL
exit /b