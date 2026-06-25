using Microsoft.Extensions.Logging;
using ZenWin.Models;
using ZenWin.Services;

namespace ZenWin.Core;

public sealed class ZenModeController(
    WindowManager windowManager,
    TaskbarManager taskbarManager,
    DesktopManager desktopManager,
    CursorManager cursorManager,
    WallpaperManager wallpaperManager,
    NotificationManager notificationManager,
    AudioManager audioManager,
    ProfileManager profileManager,
    ILogger<ZenModeController> logger)
{
    private WindowSnapshot? _snapshot;
    private ZenModeProfile _profile = ZenWinSettings.DefaultProfile;

    public bool IsActive => _snapshot is not null;
    public event EventHandler<bool>? ActiveChanged;

    public ZenModeProfile CurrentProfile => _profile;

    public bool Toggle(ZenWinSettings settings) => IsActive ? Exit() : Enter(settings);

    public bool Enter(ZenWinSettings settings)
    {
        if (IsActive)
            return true;

        var hwnd = windowManager.ActiveWindow;
        if (hwnd == nint.Zero)
            return false;

        WindowSnapshot snapshot;
        try
        {
            snapshot = windowManager.Capture(hwnd);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to capture active window.");
            return false;
        }

        _profile = profileManager.SelectProfile(settings, snapshot.ProcessName);
        if (!windowManager.TryEnterZen(snapshot, _profile))
            return false;

        _snapshot = snapshot;
        if (_profile.HideTaskbar)
            taskbarManager.Hide();
        if (_profile.HideDesktopIcons)
            desktopManager.HideIcons();
        if (_profile.HideCursor)
            cursorManager.Enable(_profile.CursorIdleSeconds);
        if (_profile.WallpaperMode != WallpaperMode.Unchanged)
            wallpaperManager.Apply(_profile.WallpaperMode);
        if (_profile.EnableAudio)
            audioManager.Play(_profile.AmbientSound);
        notificationManager.TryEnableDoNotDisturb();
        ActiveChanged?.Invoke(this, true);
        return true;
    }

    public bool Exit()
    {
        if (_snapshot is null)
            return true;

        var snapshot = _snapshot;
        _snapshot = null;
        audioManager.Stop();
        cursorManager.Disable();
        wallpaperManager.Restore();
        notificationManager.Restore();
        desktopManager.RestoreIcons();
        taskbarManager.Restore();
        windowManager.Restore(snapshot);
        ActiveChanged?.Invoke(this, false);
        return true;
    }
}
