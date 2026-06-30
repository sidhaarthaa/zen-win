using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
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

        if (!NativeMethods.GetWindowRect(hwnd, out var rect))
            throw LastWin32Exception("Unable to read the target window bounds.");

        NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
        var style = ReadWindowLong(hwnd, NativeMethods.GWL_STYLE);
        if ((style & NativeMethods.WS_CHILD) != 0)
            throw new InvalidOperationException("The selected handle is a child control, not a top-level application window.");

        var placement = new WINDOWPLACEMENT { Length = Marshal.SizeOf<WINDOWPLACEMENT>() };
        if (!NativeMethods.GetWindowPlacement(hwnd, ref placement))
            throw LastWin32Exception("Unable to read the target window placement.");

        string processName;
        try
        {
            processName = Process.GetProcessById((int)pid).ProcessName;
        }
        catch
        {
            processName = $"pid-{pid}";
        }

        return new WindowSnapshot
        {
            Handle = hwnd,
            ProcessId = pid,
            Style = style,
            ExtendedStyle = ReadWindowLong(hwnd, NativeMethods.GWL_EXSTYLE),
            Bounds = ToRectangle(rect),
            Placement = new WindowPlacementSnapshot
            {
                Flags = placement.Flags,
                ShowCommand = placement.ShowCmd,
                MinPosition = new Point(placement.MinPosition.X, placement.MinPosition.Y),
                MaxPosition = new Point(placement.MaxPosition.X, placement.MaxPosition.Y),
                NormalBounds = ToRectangle(placement.NormalPosition)
            },
            Menu = NativeMethods.GetMenu(hwnd),
            WasMaximized = NativeMethods.IsZoomed(hwnd),
            WasMinimized = NativeMethods.IsIconic(hwnd),
            ProcessName = processName,
            Title = GetTitle(hwnd)
        };
    }

    public FramelessResult TryEnterFrameless(WindowSnapshot snapshot)
    {
        if (!NativeMethods.IsWindow(snapshot.Handle))
            return FramelessResult.Failure("The target window no longer exists.");

        var result = ApplyFrameless(snapshot);
        if (result.Succeeded)
        {
            var hadStandardChrome = WindowStyleTransformer.HasStandardFrame(
                snapshot.Style,
                snapshot.ExtendedStyle) || snapshot.Menu != nint.Zero;
            return hadStandardChrome
                ? FramelessResult.Success(
                    "The standard Windows frame was removed. Client-drawn controls remain application content.")
                : FramelessResult.Success(
                    "No standard Windows frame was detected. Any visible title bar or controls are drawn inside the application and cannot be removed universally.");
        }

        logger.LogWarning("Frameless mode failed for {Process}: {Reason}", snapshot.ProcessName, result.Message);
        Restore(snapshot);
        return result;
    }

    public FramelessResult EnsureFrameless(WindowSnapshot snapshot)
    {
        if (!NativeMethods.IsWindow(snapshot.Handle))
            return FramelessResult.Failure("The target window was closed.");

        try
        {
            var currentStyle = ReadWindowLong(snapshot.Handle, NativeMethods.GWL_STYLE);
            var currentExtendedStyle = ReadWindowLong(snapshot.Handle, NativeMethods.GWL_EXSTYLE);
            var hasMenu = NativeMethods.GetMenu(snapshot.Handle) != nint.Zero;
            if (!WindowStyleTransformer.HasStandardFrame(currentStyle, currentExtendedStyle) && !hasMenu)
                return FramelessResult.Success();
        }
        catch (Exception ex)
        {
            return FramelessResult.Failure(ex.Message);
        }

        return ApplyFrameless(snapshot);
    }

    private FramelessResult ApplyFrameless(WindowSnapshot snapshot)
    {
        try
        {
            var style = WindowStyleTransformer.RemoveStandardFrame(
                ReadWindowLong(snapshot.Handle, NativeMethods.GWL_STYLE));
            var extendedStyle = WindowStyleTransformer.RemoveExtendedFrame(
                ReadWindowLong(snapshot.Handle, NativeMethods.GWL_EXSTYLE));

            SetWindowLongChecked(snapshot.Handle, NativeMethods.GWL_STYLE, style);
            SetWindowLongChecked(snapshot.Handle, NativeMethods.GWL_EXSTYLE, extendedStyle);

            if (NativeMethods.GetMenu(snapshot.Handle) != nint.Zero)
            {
                if (!NativeMethods.SetMenu(snapshot.Handle, nint.Zero))
                    throw LastWin32Exception("Windows rejected removal of the native menu.");
                NativeMethods.DrawMenuBar(snapshot.Handle);
            }

            var bounds = MonitorInfo.FromWindow(snapshot.Handle).Bounds;
            if (!NativeMethods.SetWindowPos(
                    snapshot.Handle,
                    NativeMethods.HWND_TOP,
                    bounds.Left,
                    bounds.Top,
                    bounds.Width,
                    bounds.Height,
                    NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_SHOWWINDOW))
                throw LastWin32Exception("Windows rejected the frameless bounds update.");

            var appliedStyle = ReadWindowLong(snapshot.Handle, NativeMethods.GWL_STYLE);
            var appliedExtendedStyle = ReadWindowLong(snapshot.Handle, NativeMethods.GWL_EXSTYLE);
            if (WindowStyleTransformer.HasStandardFrame(appliedStyle, appliedExtendedStyle))
                return FramelessResult.Failure(
                    "The application restored its standard frame immediately. ZenWin will not report a partial change as success.");

            if (NativeMethods.GetMenu(snapshot.Handle) != nint.Zero)
                return FramelessResult.Failure("The application restored its native menu immediately.");

            NativeMethods.SetForegroundWindow(snapshot.Handle);
            return FramelessResult.Success();
        }
        catch (Exception ex)
        {
            return FramelessResult.Failure(ex.Message);
        }
    }

    public bool Restore(WindowSnapshot snapshot)
    {
        if (!NativeMethods.IsWindow(snapshot.Handle))
            return false;

        try
        {
            SetWindowLongChecked(snapshot.Handle, NativeMethods.GWL_STYLE, snapshot.Style);
            SetWindowLongChecked(snapshot.Handle, NativeMethods.GWL_EXSTYLE, snapshot.ExtendedStyle);

            if (NativeMethods.GetMenu(snapshot.Handle) != snapshot.Menu)
            {
                if (!NativeMethods.SetMenu(snapshot.Handle, snapshot.Menu))
                    throw LastWin32Exception("Unable to restore the native menu.");
                NativeMethods.DrawMenuBar(snapshot.Handle);
            }

            var placement = new WINDOWPLACEMENT
            {
                Length = Marshal.SizeOf<WINDOWPLACEMENT>(),
                Flags = snapshot.Placement.Flags,
                ShowCmd = snapshot.Placement.ShowCommand,
                MinPosition = new POINT
                {
                    X = snapshot.Placement.MinPosition.X,
                    Y = snapshot.Placement.MinPosition.Y
                },
                MaxPosition = new POINT
                {
                    X = snapshot.Placement.MaxPosition.X,
                    Y = snapshot.Placement.MaxPosition.Y
                },
                NormalPosition = ToRect(snapshot.Placement.NormalBounds)
            };

            if (!NativeMethods.SetWindowPos(
                    snapshot.Handle,
                    NativeMethods.HWND_TOP,
                    0,
                    0,
                    0,
                    0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE |
                    NativeMethods.SWP_NOZORDER | NativeMethods.SWP_FRAMECHANGED))
                throw LastWin32Exception("Unable to recalculate the restored window frame.");

            if (!NativeMethods.SetWindowPlacement(snapshot.Handle, ref placement))
                throw LastWin32Exception("Unable to restore the target window placement.");

            NativeMethods.SetForegroundWindow(snapshot.Handle);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to restore window {Handle}", snapshot.Handle);
            return false;
        }
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

    private static int ReadWindowLong(nint hwnd, int index)
    {
        Marshal.SetLastPInvokeError(0);
        var value = NativeMethods.GetWindowLongPtr(hwnd, index);
        var error = Marshal.GetLastPInvokeError();
        if (value == nint.Zero && error != 0)
            throw new Win32Exception(error);
        return unchecked((int)value.ToInt64());
    }

    private static void SetWindowLongChecked(nint hwnd, int index, int value)
    {
        Marshal.SetLastPInvokeError(0);
        var previous = NativeMethods.SetWindowLongPtr(hwnd, index, new nint(value));
        var error = Marshal.GetLastPInvokeError();
        if (previous == nint.Zero && error != 0)
            throw new Win32Exception(error);
    }

    private static Win32Exception LastWin32Exception(string message)
    {
        var error = Marshal.GetLastPInvokeError();
        return new Win32Exception(error, $"{message} Win32 error {error}.");
    }

    private static Rectangle ToRectangle(RECT rect) =>
        Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);

    private static RECT ToRect(Rectangle rectangle) => new()
    {
        Left = rectangle.Left,
        Top = rectangle.Top,
        Right = rectangle.Right,
        Bottom = rectangle.Bottom
    };

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
