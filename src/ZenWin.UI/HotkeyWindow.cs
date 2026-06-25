using System.Windows.Interop;
using System.Windows.Input;
using ZenWin.Interop;

namespace ZenWin.UI;

public sealed class HotkeyWindow : IDisposable
{
    private readonly HwndSource _source;
    public event EventHandler<int>? HotkeyPressed;

    public HotkeyWindow()
    {
        var parameters = new HwndSourceParameters("ZenWinHotkeys") { Width = 0, Height = 0 };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    public void RegisterDefaults()
    {
        NativeMethods.RegisterHotKey(_source.Handle, 1, 0, KeyInterop.VirtualKeyFromKey(Key.F10));
        NativeMethods.RegisterHotKey(_source.Handle, 2, NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT, KeyInterop.VirtualKeyFromKey(Key.Z));
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            HotkeyPressed?.Invoke(this, wParam.ToInt32());
            handled = true;
        }
        return nint.Zero;
    }

    public void Dispose()
    {
        NativeMethods.UnregisterHotKey(_source.Handle, 1);
        NativeMethods.UnregisterHotKey(_source.Handle, 2);
        _source.Dispose();
    }
}
