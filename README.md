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

## Download From GitHub Actions

After pushing this repository to GitHub, open the repository's **Actions** tab, select the latest `build` workflow run, and download the `ZenWin-win-x64` artifact. The workflow publishes a self-contained Windows build, so the downloaded app does not require a local .NET SDK.

## Hotkeys

- `F10`: Toggle Zen Mode
- `Ctrl+Shift+Z`: Toggle toolbar

## OS constraints

Windows does not expose a supported public desktop API for toggling Focus Assist / Do Not Disturb for arbitrary unpackaged desktop applications. ZenWin logs this and leaves the user's notification setting unchanged.

Some elevated, protected, exclusive fullscreen, or custom-rendered windows reject style changes. ZenWin detects failed operations and exits cleanly rather than crashing.

## Packaging

The repository includes CI publishing for standalone Windows builds. MSIX/installer assets are scaffolded under `installer/` for signing and store identity details that must be supplied per publisher.
