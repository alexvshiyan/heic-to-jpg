#Requires -RunAsAdministrator
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$installDir = 'C:\Program Files\HeicToJpg'
$dllPath    = "$installDir\HeicToJpg.Shell.dll"
$codeBase   = "file:///$($dllPath.Replace('\','/'))"
$assembly   = 'HeicToJpg.Shell, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
$runtime    = 'v4.0.30319'

# ── CLSIDs ───────────────────────────────────────────────────────────────────
$clsidContextMenu   = '{B3D5F8A2-7C14-4E9B-A612-0D3F8B5E2C1A}'
$clsidExplorerCmd   = '{4B8E3F21-A5D7-4C9E-B823-F9D156A7E302}'

# ── Stop Explorer and dotnet processes to release locked files ───────────────
Write-Host "Stopping Explorer..."
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue

Write-Host "Stopping dotnet processes..."
Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue

Write-Host "Waiting for file handles to release..."
Start-Sleep -Seconds 3

# ── Build ─────────────────────────────────────────────────────────────────────
Write-Host "Building Release..."
$sln = Join-Path $PSScriptRoot 'HeicToJpg.sln'
& dotnet build $sln -c Release -f net48 --nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)" }

# ── Copy build output ─────────────────────────────────────────────────────────
$src = Join-Path $PSScriptRoot 'HeicToJpg.Shell\bin\Release\net48'
Write-Host "Copying from $src ..."
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Copy-Item "$src\*" $installDir -Force

# ── Helper: write InprocServer32 keys for a CLSID ────────────────────────────
function Register-NetComServer([string]$clsid, [string]$className, [string]$friendlyName, [string]$threadingModel = 'Both') {
    $base = "HKLM:\SOFTWARE\Classes\CLSID\$clsid"

    New-Item -Path $base -Force | Out-Null
    Set-ItemProperty $base '(Default)' $className

    $ipc = "$base\InprocServer32"
    New-Item -Path $ipc -Force | Out-Null
    Set-ItemProperty $ipc '(Default)'      'mscoree.dll'
    Set-ItemProperty $ipc 'ThreadingModel' $threadingModel
    Set-ItemProperty $ipc 'Class'          $className
    Set-ItemProperty $ipc 'Assembly'       $assembly
    Set-ItemProperty $ipc 'RuntimeVersion' $runtime
    Set-ItemProperty $ipc 'CodeBase'       $codeBase

    $ver = "$ipc\1.0.0.0"
    New-Item -Path $ver -Force | Out-Null
    Set-ItemProperty $ver 'Class'          $className
    Set-ItemProperty $ver 'Assembly'       $assembly
    Set-ItemProperty $ver 'RuntimeVersion' $runtime
    Set-ItemProperty $ver 'CodeBase'       $codeBase

    New-Item -Path "$base\ProgId"             -Force | Out-Null
    Set-ItemProperty    "$base\ProgId" '(Default)' $className

    # Implemented Categories: .NET component
    New-Item -Path "$base\Implemented Categories\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}" `
             -Force | Out-Null

    # Approved shell extensions list
    $approved = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved'
    Set-ItemProperty $approved $clsid $friendlyName

    Write-Host "  Registered $friendlyName ($clsid)"
}

# ── 1. IContextMenu handler (classic / Win10 fallback) ───────────────────────
Register-NetComServer $clsidContextMenu `
    'HeicToJpg.Shell.HeicContextMenuHandler' `
    'HeicToJpg Context Menu' `
    -threadingModel 'Apartment'

# Register under extension and ProgID
foreach ($root in @('HKLM:\SOFTWARE\Classes\.heic', 'HKLM:\SOFTWARE\Classes\heic.1')) {
    $key = "$root\ShellEx\ContextMenuHandlers\HeicToJpg"
    New-Item -Path $key -Force | Out-Null
    Set-ItemProperty $key '(Default)' $clsidContextMenu
}

# ── 2. IExplorerCommand handler (Windows 11 native context menu) ──────────────
Register-NetComServer $clsidExplorerCmd `
    'HeicToJpg.Shell.HeicExplorerCommand' `
    'HeicToJpg Explorer Command'

# Register verb under ProgID and extension (classic menu / verb implementation)
foreach ($root in @('HKLM:\SOFTWARE\Classes\heic.1', 'HKLM:\SOFTWARE\Classes\.heic')) {
    $verb = "$root\shell\ConvertToJpeg"
    New-Item -Path $verb -Force | Out-Null
    Set-ItemProperty $verb 'MUIVerb'               'Convert to JPEG'
    Set-ItemProperty $verb 'ExplorerCommandHandler' $clsidExplorerCmd
}

# Register as ExplorerCommandHandlers shell extension (Windows 11 modern menu)
foreach ($root in @('HKLM:\SOFTWARE\Classes\heic.1', 'HKLM:\SOFTWARE\Classes\.heic')) {
    $key = "$root\shellex\ExplorerCommandHandlers\$clsidExplorerCmd"
    New-Item -Path $key -Force | Out-Null
    Set-ItemProperty $key '(Default)' ''
    Write-Host "  Registered ExplorerCommandHandlers under $root"
}

# ── 3. Register AUMID for toast notifications ────────────────────────────────
# CreateToastNotifier(appId) requires the app ID to exist under
# HKCU\SOFTWARE\Classes\AppUserModelId\{appId} or Windows silently drops
# the notification.
$aumid = 'HKCU:\SOFTWARE\Classes\AppUserModelId\HeicToJpg.Converter'
New-Item -Path $aumid -Force | Out-Null
Set-ItemProperty $aumid 'DisplayName' 'Convert to JPEG'
Write-Host "  Registered toast AUMID"

# ── 5. Restore classic context menu for current user (Windows 11) ────────────
# IExplorerCommand requires package identity to appear in the top-level modern
# menu - without it items appear only under "Show more options". Setting this
# HKCU key restores the classic context menu globally so the IContextMenu
# handler is always visible at top level.
$optionA = 'HKCU:\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32'
New-Item -Path $optionA -Force | Out-Null
Set-ItemProperty $optionA '(Default)' ''
Write-Host "  Restored classic context menu (Option A)"

# ── 6. Restart Explorer ───────────────────────────────────────────────────────
Write-Host "Starting Explorer..."
Start-Process explorer

Write-Host "`nInstall complete."
