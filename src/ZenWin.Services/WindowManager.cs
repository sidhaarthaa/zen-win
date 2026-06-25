using System.Diagnostics;
using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging;
using ZenWin.Interop;
using ZenWin.Models;

namespace ZenWin.Services;

public sealed class WindowManager(ILogger<WindowManager> logger)
{
    public nint ActiveWindow => NativeMethods.GetForegroundWindow();

    public WindowSnapshot Capture(nint hwnd)
    {
        if (hwnd == nint.Zero || !NativeMethods.IsWindow(hwnd))
            throw new InvalidOperationException("No valid foreground window is available.");

        NativeMethods.GetWindowRect(hwnd, out var rect);
        NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
        var title = GetTitle(hwnd);
        var processName = Process.GetProcessById((int)pid).ProcessName;

        return new WindowSnapshot
        {
            Handle = hwnd,
            Style = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE).ToInt32(),
            ExtendedStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt32(),
            Bounds = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom),
            WasMaximized = NativeMethods.IsZoomed(hwnd),
            WasMinimized = NativeMethods.IsIconic(hwnd),
            InsertAfter = NativeMethods.HWND_TOP,
            ProcessName = processName,
            Title = title
        };
    }

    public bool TryEnterZen(WindowSnapshot snapshot, ZenModeProfile profile)
    {
        if (!NativeMethods.IsWindow(snapshot.Handle))
            return false;

        try
        {
            var style = snapshot.Style;
            var exStyle = snapshot.ExtendedStyle;

            if (profile.RemoveTitleBar)
                style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_SYSMENU | NativeMethods.WS_MINIMIZEBOX | NativeMethods.WS_MAXIMIZEBOX);
            if (profile.RemoveBorder)
            {
                style &= ~(NativeMethods.WS_THICKFRAME | NativeMethods.WS_BORDER | NativeMethods.WS_DLGFRAME);
                exStyle &= ~(NativeMethods.WS_EX_DLGMODALFRAME | NativeMethods.WS_EX_CLIENTEDGE | NativeMethods.WS_EX_STATICEDGE);
            }

            NativeMethods.SetWindowLongPtr(snapshot.Handle, NativeMethods.GWL_STYLE, style);
            NativeMethods.SetWindowLongPtr(snapshot.Handle, NativeMethods.GWL_EXSTYLE, exStyle);
            var monitor = MonitorInfo.FromWindow(snapshot.Handle);
            NativeMethods.SetWindowPos(
                snapshot.Handle,
                NativeMethods.HWND_TOP,
                monitor.Bounds.Left,
                monitor.Bounds.Top,
                monitor.Bounds.Width,
                monitor.Bounds.Height,
                NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_SHOWWINDOW);
            NativeMethods.ShowWindow(snapshot.Handle, NativeMethods.SW_MAXIMIZE);
            NativeMethods.SetForegroundWindow(snapshot.Handle);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to enter Zen Mode for {Handle}", snapshot.Handle);
            return false;
        }
    }

    public void Restore(WindowSnapshot snapshot)
    {
        if (!NativeMethods.IsWindow(snapshot.Handle))
            return;

        NativeMethods.SetWindowLongPtr(snapshot.Handle, NativeMethods.GWL_STYLE, snapshot.Style);
        NativeMethods.SetWindowLongPtr(snapshot.Handle, NativeMethods.GWL_EXSTYLE, snapshot.ExtendedStyle);
        NativeMethods.ShowWindow(snapshot.Handle, NativeMethods.SW_RESTORE);
        NativeMethods.SetWindowPos(
            snapshot.Handle,
            snapshot.InsertAfter,
            snapshot.Bounds.Left,
            snapshot.Bounds.Top,
            snapshot.Bounds.Width,
            snapshot.Bounds.Height,
            NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_SHOWWINDOW);

        if (snapshot.WasMaximized)
            NativeMethods.ShowWindow(snapshot.Handle, NativeMethods.SW_MAXIMIZE);
        else if (snapshot.WasMinimized)
            NativeMethods.ShowWindow(snapshot.Handle, NativeMethods.SW_HIDE);

        NativeMethods.SetForegroundWindow(snapshot.Handle);
    }

    public IReadOnlyList<nint> VisibleTopLevelWindows()
    {
        var shell = NativeMethods.GetShellWindow();
        var windows = new List<nint>();
        NativeMethods.EnumWindows((hwnd, _) =>
        {
            if (hwnd != shell && NativeMethods.IsWindowVisible(hwnd) && GetTitle(hwnd).Length > 0)
                windows.Add(hwnd);
            return true;
        }, nint.Zero);
        return windows;
    }

    private static string GetTitle(nint hwnd)
    {
        var length = NativeMethods.GetWindowTextLength(hwnd);
        if (length <= 0)
            return string.Empty;
        var builder = new StringBuilder(length + 1);
        NativeMethods.GetWindowText(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }
}
