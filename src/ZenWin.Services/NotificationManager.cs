using Microsoft.Extensions.Logging;

namespace ZenWin.Services;

public sealed class NotificationManager(ILogger<NotificationManager> logger)
{
    public bool IsFocusAssistControllable => false;

    public void TryEnableDoNotDisturb()
    {
        logger.LogInformation("Windows does not expose a supported public desktop API for toggling Focus Assist/Do Not Disturb. ZenWin leaves this setting unchanged.");
    }

    public void Restore()
    {
    }
}
