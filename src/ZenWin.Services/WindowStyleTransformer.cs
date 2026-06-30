using ZenWin.Interop;

namespace ZenWin.Services;

public static class WindowStyleTransformer
{
    public const int StandardFrameMask =
        NativeMethods.WS_CAPTION |
        NativeMethods.WS_THICKFRAME |
        NativeMethods.WS_SYSMENU |
        NativeMethods.WS_MINIMIZEBOX |
        NativeMethods.WS_MAXIMIZEBOX |
        NativeMethods.WS_BORDER |
        NativeMethods.WS_DLGFRAME;

    public const int ExtendedFrameMask =
        NativeMethods.WS_EX_DLGMODALFRAME |
        NativeMethods.WS_EX_WINDOWEDGE |
        NativeMethods.WS_EX_CLIENTEDGE |
        NativeMethods.WS_EX_STATICEDGE;

    public static int RemoveStandardFrame(int style) => style & ~StandardFrameMask;

    public static int RemoveExtendedFrame(int extendedStyle) => extendedStyle & ~ExtendedFrameMask;

    public static bool HasStandardFrame(int style, int extendedStyle) =>
        (style & StandardFrameMask) != 0 || (extendedStyle & ExtendedFrameMask) != 0;
}
