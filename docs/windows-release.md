# Windows Release Guide

This release is for running the demo on a Windows presentation computer.

## What Windows Runs

The Windows machine runs two parts:

- The Unity desktop demo app from `release/Windows/VRAvatarHeartWatch.exe`.
- The local SQLite API from `apps/api`.

The iPhone and Apple Watch app are not built on Windows. Use the iPhone app that was already installed from Xcode on the Mac.

## Does Windows Need Xcode?

No. Windows does not need Xcode.

Xcode is only needed on macOS when you want to build or reinstall the iPhone and Apple Watch app. A Windows computer cannot install or build the iOS/watchOS app directly.

## Windows Requirements

The current Windows package requires:

- Windows 10 or Windows 11.
- Node.js 20 or newer with npm.
- iPhone, Apple Watch, Quest 3, and the Windows computer on the same Wi-Fi.

The package includes all Unity demo assets and the SQLite API source. It does not include a fully bundled Windows backend executable yet, so Node.js is required to start the local API.

## Files To Copy

Copy or download the release package and keep this folder structure:

```text
apps/api
database
docs/windows-release.md
scripts/start-demo-windows.ps1
package.json
package-lock.json
release/Windows
```

Do not copy only `VRAvatarHeartWatch.exe`. Unity also needs the adjacent `VRAvatarHeartWatch_Data` folder and DLL files.

## Start On Windows

Open PowerShell in the package root and run:

```powershell
.\scripts\start-demo-windows.ps1
```

If PowerShell blocks local scripts, run:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\start-demo-windows.ps1
```

The script prints an API URL such as:

```text
http://WINDOWS_IP:8787
```

Enter that URL in the iPhone app Settings tab. Do not use `localhost` on iPhone, because `localhost` means the iPhone itself.

## Demo Flow

1. Run `scripts/start-demo-windows.ps1`.
2. Enter the printed `http://WINDOWS_IP:8787` URL in the iPhone app.
3. Open the Windows Unity app if the script did not open it automatically.
4. Press Quest right-hand **B** to start the demo.
5. Tap **Start** on Apple Watch.
6. Watch the iPhone app Overview/Data/VR tabs.
7. Press Quest right-hand **Menu** to exit VR and stop live writes.

