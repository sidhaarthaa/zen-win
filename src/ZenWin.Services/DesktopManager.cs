using ZenWin.Interop;

namespace ZenWin.Services;

public sealed class DesktopManager
{
    private nint _desktopIcons;
    private bool _hidden;

    public void HideIcons()
    {
        _desktopIcons = FindDesktopIcons();
        if (_desktopIcons != nint.Zero && NativeMethods.IsWindowVisible(_desktopIcons))
        {
            NativeMethods.ShowWindow(_desktopIcons, NativeMethods.SW_HIDE);
            _hidden = true;
        }
    }

    public void RestoreIcons()
    {
        if (_hidden && _desktopIcons != nint.Zero && NativeMethods.IsWindow(_desktopIcons))
            NativeMethods.ShowWindow(_desktopIcons, NativeMethods.SW_SHOW);
        _hidden = false;
        _desktopIcons = nint.Zero;
    }

    private static nint FindDesktopIcons()
    {
        var progman = NativeMethods.FindWindow("Progman", null);
        var defView = NativeMethods.FindWindowEx(progman, nint.Zero, "SHELLDLL_DefView", null);
        if (defView != nint.Zero)
            return NativeMethods.FindWindowEx(defView, nint.Zero, "SysListView32", "FolderView");

        nint result = nint.Zero;
        NativeMethods.EnumWindows((hwnd, _) =>
        {
            var workerDefView = NativeMethods.FindWindowEx(hwnd, nint.Zero, "SHELLDLL_DefView", null);
            if (workerDefView != nint.Zero)
            {
                result = NativeMethods.FindWindowEx(workerDefView, nint.Zero, "SysListView32", "FolderView");
                return false;
            }
            return true;
        }, nint.Zero);
        return result;
    }
}
