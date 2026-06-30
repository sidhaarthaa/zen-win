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

## How to use ZenWin

1. Start ZenWin. It remains available in the Windows notification area.
2. Focus the desktop application you want to change.
3. Press `F10`. ZenWin removes the standard frame and expands the window to its monitor.
4. Press `F10` again to restore the original frame, menu, size, position, and show state.
5. To close ZenWin, right-click its notification-area icon and select **Exit**. ZenWin
   restores the active target before exiting.

### Hotkeys

| Hotkey | Action |
| --- | --- |
| `F10` | Toggle frameless mode for the focused application; press again to restore it |

`F10` is system-wide while ZenWin is running. If another application has already
registered `F10`, ZenWin displays an error at startup. Close or reconfigure the
conflicting application, then restart ZenWin.

Only one target window is managed at a time. Restore the current target before focusing
another application and enabling frameless mode.

### Troubleshooting

- **Nothing happens:** confirm ZenWin is running in the notification area and that the
  target application, not ZenWin, is focused when you press `F10`.
- **Access denied or the frame remains:** if the target runs as administrator, ZenWin
  must run at the same elevation level.
- **The title bar remains but the window fills the monitor:** the application is likely
  drawing that title bar inside its client area. See the limitations below.
- **How do I close a frameless app?** press `F10` to restore its caption buttons, or use
  the application's own shortcut such as `Alt+F4`.
- **ZenWin or the target closes unexpectedly:** starting ZenWin again does not have the
  original in-memory snapshot. Windows normally recreates standard styles when the
  target restarts; otherwise close and reopen the target application.

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
