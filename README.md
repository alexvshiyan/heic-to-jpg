# heic-to-jpg

Windows Shell Extension that adds **Convert to JPEG** to the right-click context menu for `.heic` files. Converts single files or batch selections in place, preserving EXIF data by default.

## Requirements

- Windows 10 or Windows 11
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) (included in Windows 11; available as a Windows Update on Windows 10)
- Administrator rights for installation

## Installation

1. Clone or download this repository.
2. Open PowerShell **as Administrator**.
3. Run:
   ```powershell
   .\install.ps1
   ```

The script builds the project in Release mode, copies files to `C:\Program Files\HeicToJpg\`, registers the COM shell extension, and restarts Explorer.

## Usage

Right-click one or more `.heic` files in Explorer and choose **Convert to JPEG**. The output `.jpg` files are saved in the same folder as the originals. Existing files are never overwritten — if `photo.jpg` already exists, the output is named `photo (1).jpg`, then `photo (2).jpg`, and so on.

A toast notification appears when conversion starts (for multiple files) and again when it finishes with a summary.

## Configuration

Quality and EXIF settings are stored in `%AppData%\HeicToJpg\config.json`, which is created automatically on first use with these defaults:

```json
{
  "JpegQuality": 85,
  "PreserveExif": true
}
```

Edit the file in any text editor to change the settings. Changes take effect immediately — no reinstallation needed.

| Setting | Values | Default |
|---|---|---|
| `JpegQuality` | 1–100 | `85` |
| `PreserveExif` | `true` / `false` | `true` |

## Uninstallation

Open a Command Prompt **as Administrator** and run:

```cmd
unregister.bat
```

This removes the shell extension registration, deletes `C:\Program Files\HeicToJpg\`, and restarts Explorer. The config file at `%AppData%\HeicToJpg\config.json` is left in place.

## Known Limitations

**Windows 11 "Show more options":** Windows 11 shows third-party shell extensions under the secondary **Show more options** menu by default. `install.ps1` automatically sets a registry key that restores the classic context menu globally, so **Convert to JPEG** appears at the top level. If you prefer to keep the modern menu, remove the following key after installation:

```
HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32
```

Note that removing it will move the item back under **Show more options**.
