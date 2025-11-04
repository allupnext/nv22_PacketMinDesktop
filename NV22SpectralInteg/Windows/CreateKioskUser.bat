@echo off

SETLOCAL



:: === CHECK FOR ADMIN PRIVILEGES ===

NET SESSION >nul 2>&1

IF %ERRORLEVEL% NEQ 0 (

    echo.

    echo Requesting Administrator privileges to run step 1...

    powershell -Command "Start-Process -FilePath '%~dpnx0' -Verb RunAs"

    exit /b

)



echo.

echo === 1. Creating Kiosk User: KioskUser (No Password) ===

:: Create the user with no password

net user KioskUser /add

if %errorlevel% neq 0 (

    echo Error creating user. Script aborted.

    pause

    goto :EOF

)

echo KioskUser created successfully.



:: Set password to never expire (Prevents Windows from forcing a change)

wmic useraccount where "Name='KioskUser'" set PasswordExpires=FALSE >nul



:: --- Set Application Permissions (Step 6 of original request) ---

:: !!! CHANGE THIS PATH to your actual application path !!!

set "AppPath=C:\Program Files\MyApp\MyApp.exe"



echo.

echo === Granting Read/Execute Permissions to KioskUser ===

echo Granting permissions on: "%AppPath%"

:: icacls grants Read/Execute (RX) permissions to the KioskUser on the app path

icacls "%AppPath%" /grant KioskUser:(RX)



if %errorlevel% neq 0 (

    echo Error granting permissions. Check if the file path is correct.

) else (

    echo Permissions granted successfully.

)



echo.

echo === TEMPORARILY SETTING AUTO-LOGIN FOR PROFILE CREATION ===

set "RegPath=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"

set "KioskUserName=KioskUser"



:: Set KioskUser as the default user for Auto-Logon

REG ADD "%RegPath%" /v DefaultUserName /t REG_SZ /d "%KioskUserName%" /f >nul

:: Set AutoAdminLogon to '1' (enabled)

REG ADD "%RegPath%" /v AutoAdminLogon /t REG_SZ /d "1" /f >nul

:: DefaultPassword is left intentionally BLANK (required for no-password auto-logon)

REG ADD "%RegPath%" /v DefaultPassword /t REG_SZ /d "" /f >nul



echo Auto-Login configured for the next boot.



echo.

echo =======================================================

echo **Action Required (Manual Login Needed!):**

echo 1. The PC will now **SHUT DOWN**.

echo 2. **Manually turn the PC back on.**

echo 3. The system will **automatically log in** as KioskUser (creating the profile) and immediately **log back out**.

echo 4. **Log in as an ADMINISTRATOR** user.

echo 5. Run the **2-ConfigureKioskShell.bat** file.

echo =======================================================

pause



:: --- YOUR REQUESTED MODIFICATION FOR COUNTDOWN AND STYLE ---

echo **System is shutting down now...**



:: PowerShell command to show a styled message for 5 seconds before shutdown
powershell -Command ^
"Add-Type -AssemblyName System.Windows.Forms; ^
$form = New-Object System.Windows.Forms.Form; ^
$form.Text = 'System Shutdown'; ^
$form.Size = New-Object System.Drawing.Size(700,200); ^
$form.StartPosition = 'CenterScreen'; ^
$form.TopMost = $true; ^
$label = New-Object System.Windows.Forms.Label; ^
$label.Text = 'Your system is shutting down in a few seconds...'; ^
$label.Font = New-Object System.Drawing.Font('Segoe UI', 16, [System.Drawing.FontStyle]::Bold); ^
$label.AutoSize = $false; ^
$label.TextAlign = 'MiddleCenter'; ^
$label.Size = New-Object System.Drawing.Size(680,100); ^
$label.Location = New-Object System.Drawing.Point(10,40); ^
$form.Controls.Add($label); ^
$timer = New-Object System.Windows.Forms.Timer; ^
$timer.Interval = 5000; ^  # 5 seconds
$timer.Add_Tick({ $timer.Stop(); $form.Close(); }); ^
$timer.Start(); ^
[void]$form.ShowDialog()"

:: Shutdown immediately after the 5-second message
shutdown /s /t 0


ENDLOCAL