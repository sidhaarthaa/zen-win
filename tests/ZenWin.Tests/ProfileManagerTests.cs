using ZenWin.Models;
using ZenWin.Services;
using Xunit;

namespace ZenWin.Tests;

public sealed class ProfileManagerTests
{
    [Fact]
    public void SelectProfile_ReturnsMatchingProcessProfile()
    {
        var manager = new ProfileManager();
        var chrome = new ZenModeProfile { Id = "chrome", Name = "Chrome", ProcessName = "chrome", RemoveTitleBar = true };
        var settings = new ZenWinSettings { Profiles = [ZenWinSettings.DefaultProfile, chrome] };

        var profile = manager.SelectProfile(settings, "chrome");

        Assert.Equal("chrome", profile.Id);
    }

    [Fact]
    public void SelectProfile_FallsBackToDefault()
    {
        var manager = new ProfileManager();
        var settings = new ZenWinSettings();

        var profile = manager.SelectProfile(settings, "unknown");

        Assert.Equal("default", profile.Id);
    }
}
