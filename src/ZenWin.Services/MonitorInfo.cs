using System.Drawing;
using ZenWin.Interop;

namespace ZenWin.Services;

public sealed record MonitorInfo(Rectangle Bounds, Rectangle WorkArea)
{
    public static MonitorInfo FromWindow(nint hwnd)
    {
        var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
        var info = new MONITORINFO { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<MONITORINFO>() };
        if (!NativeMethods.GetMonitorInfo(monitor, ref info))
            throw new InvalidOperationException("Unable to resolve monitor information.");

        return new MonitorInfo(
            Rectangle.FromLTRB(info.rcMonitor.Left, info.rcMonitor.Top, info.rcMonitor.Right, info.rcMonitor.Bottom),
            Rectangle.FromLTRB(info.rcWork.Left, info.rcWork.Top, info.rcWork.Right, info.rcWork.Bottom));
    }
}
