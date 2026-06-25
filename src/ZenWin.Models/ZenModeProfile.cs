namespace ZenWin.Models;

public sealed class ZenModeProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Default";
    public string? ProcessName { get; set; }
    public bool RemoveTitleBar { get; set; } = true;
    public bool RemoveBorder { get; set; } = true;
    public bool HideTaskbar { get; set; } = true;
    public bool HideDesktopIcons { get; set; } = true;
    public bool DimBackgroundWindows { get; set; } = true;
    public double BackgroundOpacity { get; set; } = 0.15;
    public bool HideCursor { get; set; } = true;
    public int CursorIdleSeconds { get; set; } = 3;
    public bool EnableAudio { get; set; }
    public AmbientSound AmbientSound { get; set; } = AmbientSound.BrownNoise;
    public WallpaperMode WallpaperMode { get; set; } = WallpaperMode.Unchanged;
}
