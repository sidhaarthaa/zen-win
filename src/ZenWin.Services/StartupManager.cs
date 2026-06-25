using Microsoft.Win32;

namespace ZenWin.Services;

public sealed class StartupManager
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ZenWin";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(ValueName) is string;
    }

    public void SetEnabled(bool enabled, string executablePath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true) ?? Registry.CurrentUser.CreateSubKey(RunKey, true);
        if (enabled)
            key.SetValue(ValueName, $"\"{executablePath}\"");
        else
            key.DeleteValue(ValueName, false);
    }
}
