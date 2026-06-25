using Microsoft.Extensions.Logging;
using ZenWin.Interop;

namespace ZenWin.Services;

public sealed class TaskbarManager(ILogger<TaskbarManager> logger)
{
    private readonly List<nint> _hidden = [];

    public void Hide()
    {
        HideWindow("Shell_TrayWnd");
        HideWindow("Shell_SecondaryTrayWnd");
    }

    public void Restore()
    {
        foreach (var hwnd in _hidden.Where(hwnd => NativeMethods.IsWindow(hwnd)))
            NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOW);
        _hidden.Clear();
    }

    private void HideWindow(string className)
    {
        var hwnd = nint.Zero;
        while ((hwnd = NativeMethods.FindWindowEx(nint.Zero, hwnd, className, null)) != nint.Zero)
        {
            if (NativeMethods.IsWindowVisible(hwnd))
            {
                logger.LogDebug("Hiding taskbar window {Handle}", hwnd);
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);
                _hidden.Add(hwnd);
            }
        }
    }
}
