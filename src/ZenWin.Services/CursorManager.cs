using System.Runtime.InteropServices;
using ZenWin.Interop;

namespace ZenWin.Services;

public sealed class CursorManager : IDisposable
{
    private readonly System.Timers.Timer _timer = new(250);
    private bool _enabled;
    private bool _hidden;
    private int _idleSeconds = 3;

    public CursorManager()
    {
        _timer.Elapsed += (_, _) => Tick();
    }

    public void Enable(int idleSeconds)
    {
        _idleSeconds = Math.Max(1, idleSeconds);
        _enabled = true;
        _timer.Start();
    }

    public void Disable()
    {
        _enabled = false;
        _timer.Stop();
        Show();
    }

    private void Tick()
    {
        if (!_enabled)
            return;

        var info = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>() };
        if (!NativeMethods.GetLastInputInfo(ref info))
            return;

        var idleMs = unchecked(NativeMethods.GetTickCount() - info.dwTime);
        if (idleMs >= _idleSeconds * 1000)
            Hide();
        else
            Show();
    }

    private void Hide()
    {
        if (_hidden)
            return;
        while (NativeMethods.ShowCursor(false) >= 0) { }
        _hidden = true;
    }

    private void Show()
    {
        if (!_hidden)
            return;
        while (NativeMethods.ShowCursor(true) < 0) { }
        _hidden = false;
    }

    public void Dispose()
    {
        Disable();
        _timer.Dispose();
    }
}
