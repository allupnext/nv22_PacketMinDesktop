@echo off
SETLOCAL

:: --- USER VARIABLES ---
set "KioskUserName=KioskUser"
set "RegPathWinlogon=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"

:: === CHECK FOR ADMIN PRIVILEGES (Required for all commands) ===
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    powershell -Command "Start-Process -FilePath '%~dpnx0' -Verb RunAs"
    exit /b
)

echo.
echo === DELETING KIOSK USER AND PROFILE ===
wmic path Win32_UserProfile where "LocalPath like '%%\\KioskUser'" delete /failfast
net user "%KioskUserName%" /delete

echo.
echo === REMOVING TEMPORARY AUTO-LOGON SETTINGS ===
REG DELETE "%RegPathWinlogon%" /v DefaultUserName /f >nul 2>&1
REG DELETE "%RegPathWinlogon%" /v AutoAdminLogon /f >nul 2>&1
REG DELETE "%RegPathWinlogon%" /v DefaultPassword /f >nul 2>&1

echo Cleanup complete.
pause

ENDLOCAL