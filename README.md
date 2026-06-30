# ZenWin

ZenWin is a Windows utility that removes the standard non-client frame from the
currently focused desktop application. Press `F10` to toggle frameless mode.

## Frameless behavior

For a conventional Win32 desktop window, ZenWin:

- removes the title bar, caption buttons, resize border, dialog frame, and DWM edge styles;
- detaches a native Win32 menu bar;
- resizes the content window to the full bounds of its current monitor;
- checks that Windows accepted the style change instead of reporting false success;
- reapplies the frameless styles if the application recreates its standard frame; and
- restores the original styles, native menu, show state, and normal window placement.

ZenWin does not change wallpaper, audio, notifications, desktop icons, cursor behavior,
or add an overlay or toolbar.

## Install

Download the native installer for your architecture from
[GitHub Releases](https://github.com/sidhaarthaa/zen-win/releases/latest).

The PowerShell installer is also available:

```powershell
irm https://raw.githubusercontent.com/sidhaarthaa/zen-win/master/installer/install.ps1 | iex
```

## Windows limitations

Windows exposes standard non-client chrome through window styles, which ZenWin can
remove reliably from ordinary Win32, WPF, and Windows Forms top-level windows.

Some visible UI is not non-client chrome:

- Chromium, Electron, WinUI, games, and other applications may draw their own title
  bar and buttons inside the client area. Removing that UI requires application-specific
  support and cannot be done safely through universal Win32 window styles.
- Browser tabs, address bars, menus implemented as controls, ribbons, and toolbars are
  application content. ZenWin does not hide or crop them.
- Elevated applications reject changes from a non-elevated ZenWin process because of
  Windows integrity-level isolation. Run ZenWin at the same elevation level when needed.
- Protected, anti-cheat, sandboxed, exclusive-fullscreen, or frequently recreated
  windows may reject or overwrite external style changes.

For these cases ZenWin removes every standard frame component Windows allows, reports
failure when the frame remains, and leaves client-drawn content intact rather than using
unsafe process injection or brittle application-specific hacks.

## Build

Install the .NET 9 SDK and run:

```powershell
dotnet restore .\ZenWin.sln
dotnet build .\ZenWin.sln -c Release
dotnet test .\ZenWin.sln -c Release
```
