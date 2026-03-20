@echo off
setlocal

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Run as Administrator.
    pause & exit /b 1
)

set "INSTALL_DIR=C:\Program Files\HeicToJpg"
set "DLL=%INSTALL_DIR%\HeicToJpg.Shell.dll"
set "CLSID={B3D5F8A2-7C14-4E9B-A612-0D3F8B5E2C1A}"
set "REGASM=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"

echo.
echo === Removing .heic handler ===
reg delete "HKLM\SOFTWARE\Classes\.heic\ShellEx\ContextMenuHandlers\HeicToJpg" /f 2>nul && echo OK || echo (already absent)

echo.
echo === Removing heic.1 handler ===
reg delete "HKLM\SOFTWARE\Classes\heic.1\ShellEx\ContextMenuHandlers\HeicToJpg" /f 2>nul && echo OK || echo (already absent)

echo.
echo === Removing Approved entry ===
reg delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" /v "%CLSID%" /f 2>nul && echo OK || echo (already absent)

echo.
echo === Unregistering COM server ===
if exist "%DLL%" (
    "%REGASM%" "%DLL%" /unregister /nologo
) else (
    reg delete "HKLM\SOFTWARE\Classes\CLSID\%CLSID%" /f 2>nul && echo OK || echo (already absent)
)

echo.
echo === Restarting Explorer ===
taskkill /f /im explorer.exe >nul 2>&1
start explorer.exe

echo.
echo === Removing install folder ===
if exist "%INSTALL_DIR%" (
    rmdir /s /q "%INSTALL_DIR%"
    echo OK
) else (
    echo (already absent)
)

echo.
echo Done.
pause
