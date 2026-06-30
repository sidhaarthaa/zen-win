# ZenWin

ZenWin is a native Windows utility that applies a universal Zen Mode to the currently focused desktop application.

The first production slice is implemented as a .NET 9 WPF solution with a clean service split for Win32 interop, settings, profiles, tray integration, toolbar, hotkeys, cursor idle hiding, taskbar and desktop icon visibility, wallpaper restore, generated ambient audio, and exact window style/bounds restore.

## Build

Install the .NET 9 SDK and run:

```powershell
dotnet restore .\ZenWin.sln
dotnet build .\ZenWin.sln -c Release
dotnet test .\ZenWin.sln -c Release
```

Run the app locally:

```powershell
dotnet run --project .\src\ZenWin.UI\ZenWin.UI.csproj
```

## Install

Install the latest release directly from GitHub (no administrator access or .NET SDK required):

```powershell
irm https://raw.githubusercontent.com/sidhaarthaa/zen-win/master/installer/install.ps1 | iex
```

The installer detects x64 or ARM64 Windows, installs under the current user's local
app data, creates Start Menu and desktop shortcuts, and registers ZenWin in Windows'
Installed Apps list.

To install manually, download the ZIP for your architecture from
[GitHub Releases](https://github.com/sidhaarthaa/zen-win/releases/latest), extract it,
and run `ZenWin.UI.exe`.

To uninstall, use **Settings > Apps > Installed apps > ZenWin**, or run:

```powershell
& "$env:LOCALAPPDATA\Programs\ZenWin\install.ps1" -Uninstall
```

## Development Builds

Open the repository's **Actions** tab, select the latest `build` workflow run, and
download the artifact for your architecture. Tagged builds such as `v0.1.0` are
published automatically to GitHub Releases.

## Hotkeys

- `F10`: Toggle Zen Mode
- `Ctrl+Shift+Z`: Toggle toolbar

## OS constraints

Windows does not expose a supported public desktop API for toggling Focus Assist / Do Not Disturb for arbitrary unpackaged desktop applications. ZenWin logs this and leaves the user's notification setting unchanged.

Some elevated, protected, exclusive fullscreen, or custom-rendered windows reject style changes. ZenWin detects failed operations and exits cleanly rather than crashing.

## Packaging

The repository publishes self-contained x64 and ARM64 ZIPs to GitHub Releases.
Winget and Chocolatey manifests can be added after the first stable release URL exists.
