using ZenWin.Models;

namespace ZenWin.Services;

public sealed class ProfileManager
{
    public ZenModeProfile SelectProfile(ZenWinSettings settings, string? processName)
    {
        if (!string.IsNullOrWhiteSpace(processName))
        {
            var match = settings.Profiles.FirstOrDefault(p =>
                string.Equals(p.ProcessName, processName, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                return match;
        }
        return settings.Profiles.FirstOrDefault(p => p.Id == "default") ?? ZenWinSettings.DefaultProfile;
    }

    public ZenModeProfile Next(ZenWinSettings settings, ZenModeProfile current)
    {
        if (settings.Profiles.Count == 0)
            return ZenWinSettings.DefaultProfile;
        var index = settings.Profiles.FindIndex(p => p.Id == current.Id);
        return settings.Profiles[(index + 1 + settings.Profiles.Count) % settings.Profiles.Count];
    }

    public ZenModeProfile Previous(ZenWinSettings settings, ZenModeProfile current)
    {
        if (settings.Profiles.Count == 0)
            return ZenWinSettings.DefaultProfile;
        var index = settings.Profiles.FindIndex(p => p.Id == current.Id);
        return settings.Profiles[(index - 1 + settings.Profiles.Count) % settings.Profiles.Count];
    }
}
