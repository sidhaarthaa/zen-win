using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using ZenWin.Interop;
using ZenWin.Models;

namespace ZenWin.Services;

public sealed class WallpaperManager
{
    private string? _originalWallpaper;
    private string? _generatedWallpaper;

    public void Apply(WallpaperMode mode)
    {
        if (mode == WallpaperMode.Unchanged)
            return;

        var buffer = new StringBuilder(260);
        NativeMethods.SystemParametersInfo(NativeMethods.SPI_GETDESKWALLPAPER, buffer.Capacity, buffer, 0);
        _originalWallpaper = buffer.ToString();

        _generatedWallpaper = Path.Combine(Path.GetTempPath(), $"zenwin-wallpaper-{mode}.bmp");
        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);
        if (mode == WallpaperMode.Gradient)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                var color = Color.FromArgb(255, Math.Min(64, y * 2), Math.Min(96, y * 3), Math.Min(80, y * 2));
                using var pen = new Pen(color);
                graphics.DrawLine(pen, 0, y, bitmap.Width, y);
            }
        }
        else
        {
            graphics.Clear(mode == WallpaperMode.Black ? Color.Black : Color.FromArgb(24, 24, 24));
        }
        bitmap.Save(_generatedWallpaper, ImageFormat.Bmp);
        NativeMethods.SystemParametersInfo(NativeMethods.SPI_SETDESKWALLPAPER, 0, _generatedWallpaper, NativeMethods.SPIF_UPDATEINIFILE | NativeMethods.SPIF_SENDCHANGE);
    }

    public void Restore()
    {
        if (!string.IsNullOrWhiteSpace(_originalWallpaper))
            NativeMethods.SystemParametersInfo(NativeMethods.SPI_SETDESKWALLPAPER, 0, _originalWallpaper, NativeMethods.SPIF_UPDATEINIFILE | NativeMethods.SPIF_SENDCHANGE);
        _originalWallpaper = null;
        if (_generatedWallpaper is not null && File.Exists(_generatedWallpaper))
            File.Delete(_generatedWallpaper);
        _generatedWallpaper = null;
    }
}
