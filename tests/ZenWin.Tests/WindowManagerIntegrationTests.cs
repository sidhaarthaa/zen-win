using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging.Abstractions;
using ZenWin.Interop;
using ZenWin.Services;
using Xunit;

namespace ZenWin.Tests;

public sealed class WindowManagerIntegrationTests
{
    private const uint MfString = 0x00000000;

    [Fact]
    public void FramelessTransaction_RemovesAndRestoresNativeFrameAndMenu()
    {
        using var ready = new ManualResetEventSlim();
        Form? form = null;
        Exception? threadFailure = null;

        var thread = new Thread(() =>
        {
            try
            {
                form = new Form
                {
                    Text = "ZenWin integration target",
                    FormBorderStyle = FormBorderStyle.Sizable,
                    StartPosition = FormStartPosition.Manual,
                    Bounds = new Rectangle(120, 140, 800, 600)
                };
                form.Shown += (_, _) => ready.Set();
                Application.Run(form);
            }
            catch (Exception ex)
            {
                threadFailure = ex;
                ready.Set();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        Assert.True(ready.Wait(TimeSpan.FromSeconds(10)));
        Assert.Null(threadFailure);
        Assert.NotNull(form);

        var menu = CreateMenu();
        Assert.NotEqual(nint.Zero, menu);
        Assert.True(AppendMenu(menu, MfString, 1, "File"));
        Assert.True(NativeMethods.SetMenu(form.Handle, menu));
        NativeMethods.DrawMenuBar(form.Handle);

        var manager = new WindowManager(NullLogger<WindowManager>.Instance);
        var snapshot = manager.Capture(form.Handle);

        try
        {
            var result = manager.TryEnterFrameless(snapshot);

            Assert.True(result.Succeeded, result.Message);
            var style = NativeMethods.GetWindowLongPtr(form.Handle, NativeMethods.GWL_STYLE).ToInt32();
            var extendedStyle = NativeMethods.GetWindowLongPtr(form.Handle, NativeMethods.GWL_EXSTYLE).ToInt32();
            Assert.False(WindowStyleTransformer.HasStandardFrame(style, extendedStyle));
            Assert.Equal(nint.Zero, NativeMethods.GetMenu(form.Handle));

            var monitor = MonitorInfo.FromWindow(form.Handle);
            Assert.True(NativeMethods.GetWindowRect(form.Handle, out var framelessRect));
            Assert.Equal(monitor.Bounds, Rectangle.FromLTRB(
                framelessRect.Left,
                framelessRect.Top,
                framelessRect.Right,
                framelessRect.Bottom));
        }
        finally
        {
            Assert.True(manager.Restore(snapshot));
            Assert.Equal(snapshot.Style, NativeMethods.GetWindowLongPtr(form.Handle, NativeMethods.GWL_STYLE).ToInt32());
            Assert.Equal(snapshot.ExtendedStyle, NativeMethods.GetWindowLongPtr(form.Handle, NativeMethods.GWL_EXSTYLE).ToInt32());
            Assert.Equal(menu, NativeMethods.GetMenu(form.Handle));

            form.BeginInvoke(form.Close);
            Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
            DestroyMenu(menu);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CreateMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool AppendMenu(nint menu, uint flags, nuint itemId, string itemText);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(nint menu);
}
