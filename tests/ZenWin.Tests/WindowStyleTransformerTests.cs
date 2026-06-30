using ZenWin.Interop;
using ZenWin.Services;
using Xunit;

namespace ZenWin.Tests;

public sealed class WindowStyleTransformerTests
{
    [Fact]
    public void RemoveStandardFrame_RemovesAllOperatingSystemChrome()
    {
        const int unrelatedStyle = 0x10000000;
        var style = unrelatedStyle | WindowStyleTransformer.StandardFrameMask;

        var frameless = WindowStyleTransformer.RemoveStandardFrame(style);

        Assert.Equal(0, frameless & WindowStyleTransformer.StandardFrameMask);
        Assert.NotEqual(0, frameless & unrelatedStyle);
    }

    [Fact]
    public void RemoveExtendedFrame_RemovesAllEdgeStyles()
    {
        const int unrelatedExtendedStyle = 0x00000008;
        var style = unrelatedExtendedStyle | WindowStyleTransformer.ExtendedFrameMask;

        var frameless = WindowStyleTransformer.RemoveExtendedFrame(style);

        Assert.Equal(0, frameless & WindowStyleTransformer.ExtendedFrameMask);
        Assert.NotEqual(0, frameless & unrelatedExtendedStyle);
    }

    [Fact]
    public void HasStandardFrame_DetectsStandardAndExtendedChrome()
    {
        Assert.True(WindowStyleTransformer.HasStandardFrame(NativeMethods.WS_CAPTION, 0));
        Assert.True(WindowStyleTransformer.HasStandardFrame(0, NativeMethods.WS_EX_WINDOWEDGE));
        Assert.False(WindowStyleTransformer.HasStandardFrame(0x10000000, 0x00000008));
    }
}
