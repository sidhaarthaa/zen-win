namespace ZenWin.Models;

public sealed class ZenWinSettings
{
    public bool StartWithWindows { get; set; }
    public bool GlobalMode { get; set; }
    public bool EnableAnimations { get; set; } = true;
    public bool EnableEscapeToExit { get; set; } = true;
    public string ToggleZenHotkey { get; set; } = "F10";
    public string ToggleToolbarHotkey { get; set; } = "Ctrl+Shift+Z";
    public string PreviousProfileHotkey { get; set; } = "Ctrl+Alt+Left";
    public string NextProfileHotkey { get; set; } = "Ctrl+Alt+Right";
    public TimerPreset TimerPreset { get; set; } = TimerPreset.Pomodoro25;
    public int CustomFocusMinutes { get; set; } = 25;
    public int CustomBreakMinutes { get; set; } = 5;
    public AppTheme Theme { get; set; } = AppTheme.System;
    public List<ZenModeProfile> Profiles { get; set; } = [DefaultProfile];

    public static ZenModeProfile DefaultProfile { get; } = new()
    {
        Id = "default",
        Name = "Default",
        ProcessName = null
    };
}
