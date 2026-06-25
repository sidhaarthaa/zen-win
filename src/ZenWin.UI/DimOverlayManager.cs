using ZenWin.Core;
using ZenWin.Services;

namespace ZenWin.UI;

public sealed class DimOverlayManager(ZenModeController controller, WindowManager windowManager)
{
    private DimOverlayWindow? _overlay;

    public void ShowForActiveWindow()
    {
        var profile = controller.CurrentProfile;
        if (!profile.DimBackgroundWindows)
            return;

        var hwnd = windowManager.ActiveWindow;
        if (hwnd == nint.Zero)
            return;

        var monitor = MonitorInfo.FromWindow(hwnd);
        _overlay = new DimOverlayWindow(monitor.Bounds, profile.BackgroundOpacity);
        _overlay.Show();
        ZenWin.Interop.NativeMethods.SetForegroundWindow(hwnd);
    }

    public void Hide()
    {
        _overlay?.Close();
        _overlay = null;
    }
}
