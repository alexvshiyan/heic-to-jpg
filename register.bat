@echo off
setlocal

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Run as Administrator.
    pause & exit /b 1
)

set "SRC_DIR=%~dp0HeicToJpg.Shell\bin\Release\net48"
set "INSTALL_DIR=C:\Program Files\HeicToJpg"
set "DLL=%INSTALL_DIR%\HeicToJpg.Shell.dll"
set "CLSID={B3D5F8A2-7C14-4E9B-A612-0D3F8B5E2C1A}"
set "REGASM=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"

echo.
echo === Checking build output ===
if not exist "%SRC_DIR%\HeicToJpg.Shell.dll" (
    echo FAIL: Build output not found at %SRC_DIR%
    pause & exit /b 1
)
echo OK: %SRC_DIR%

echo.
echo === Installing to %INSTALL_DIR% ===
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
xcopy /y /q "%SRC_DIR%\*" "%INSTALL_DIR%\"
if %errorLevel% neq 0 ( echo FAIL: xcopy failed & pause & exit /b 1 )
echo OK

echo.
echo === Unblocking DLLs (remove zone identifiers from OneDrive) ===
powershell -Command "Get-ChildItem '%INSTALL_DIR%\*.dll' | Unblock-File"
echo OK

echo.
echo === Registering with 64-bit regasm ===
"%REGASM%" "%DLL%" /codebase /nologo
if %errorLevel% neq 0 ( echo FAIL: regasm & pause & exit /b 1 )
echo OK

echo.
echo === Setting ThreadingModel = Apartment ===
reg add "HKLM\SOFTWARE\Classes\CLSID\%CLSID%\InprocServer32" /v "ThreadingModel" /d "Apartment" /f
if %errorLevel% neq 0 ( echo FAIL & pause & exit /b 1 )

echo.
echo === Approving shell extension ===
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" /v "%CLSID%" /d "HeicToJpg Context Menu" /f
if %errorLevel% neq 0 ( echo FAIL & pause & exit /b 1 )

echo.
echo === Writing .heic handler ===
reg add "HKLM\SOFTWARE\Classes\.heic\ShellEx\ContextMenuHandlers\HeicToJpg" /ve /d "%CLSID%" /f
if %errorLevel% neq 0 ( echo FAIL & pause & exit /b 1 )

echo.
echo === Writing heic.1 handler (ProgID) ===
reg add "HKLM\SOFTWARE\Classes\heic.1\ShellEx\ContextMenuHandlers\HeicToJpg" /ve /d "%CLSID%" /f
if %errorLevel% neq 0 ( echo FAIL & pause & exit /b 1 )

echo.
echo === Writing SystemFileAssociations handler (works regardless of UserChoice/Store app) ===
reg add "HKLM\SOFTWARE\Classes\SystemFileAssociations\.heic\ShellEx\ContextMenuHandlers\HeicToJpg" /ve /d "%CLSID%" /f
if %errorLevel% neq 0 ( echo FAIL & pause & exit /b 1 )

echo.
echo === Verifying ===
reg query "HKLM\SOFTWARE\Classes\SystemFileAssociations\.heic\ShellEx\ContextMenuHandlers\HeicToJpg"
if %errorLevel% neq 0 ( echo FAIL: SystemFileAssociations key not found! & pause & exit /b 1 )
echo OK: SystemFileAssociations key confirmed.

echo.
echo === Restarting Explorer ===
taskkill /f /im explorer.exe >nul 2>&1
start explorer.exe

echo.
echo Done. DLLs installed to %INSTALL_DIR%
pause
