using Microsoft.Extensions.Logging;
using ZenWin.Models;
using ZenWin.Services;

namespace ZenWin.Core;

public sealed class ZenModeController(
    WindowManager windowManager,
    ILogger<ZenModeController> logger) : IDisposable
{
    private readonly object _sync = new();
    private WindowSnapshot? _snapshot;
    private Timer? _enforcementTimer;

    public bool IsActive
    {
        get
        {
            lock (_sync)
                return _snapshot is not null;
        }
    }

    public string StatusMessage { get; private set; } = "Focus an application and press F10.";
    public event EventHandler<bool>? ActiveChanged;

    public bool Toggle() => IsActive ? Exit() : Enter();

    public bool Enter()
    {
        lock (_sync)
        {
            if (_snapshot is not null)
                return true;

            WindowSnapshot snapshot;
            try
            {
                snapshot = windowManager.Capture(windowManager.ActiveWindow);
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                logger.LogWarning(ex, "Unable to capture the active window.");
                return false;
            }

            if (snapshot.ProcessId == Environment.ProcessId)
            {
                StatusMessage = "Focus another application before enabling frameless mode.";
                return false;
            }

            var result = windowManager.TryEnterFrameless(snapshot);
            StatusMessage = result.Message;
            if (!result.Succeeded)
                return false;

            _snapshot = snapshot;
            _enforcementTimer = new Timer(EnforceFrameless, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        ActiveChanged?.Invoke(this, true);
        return true;
    }

    public bool Exit()
    {
        WindowSnapshot? snapshot;
        lock (_sync)
        {
            snapshot = _snapshot;
            if (snapshot is null)
                return true;

            _snapshot = null;
            _enforcementTimer?.Dispose();
            _enforcementTimer = null;
        }

        var restored = windowManager.Restore(snapshot);
        StatusMessage = restored
            ? "The original window frame was restored."
            : "The application closed or Windows rejected part of the frame restoration.";
        ActiveChanged?.Invoke(this, false);
        return restored;
    }

    private void EnforceFrameless(object? state)
    {
        _ = state;
        WindowSnapshot? snapshot;
        lock (_sync)
            snapshot = _snapshot;

        if (snapshot is null)
            return;

        var result = windowManager.EnsureFrameless(snapshot);
        if (!result.Succeeded)
        {
            StatusMessage = result.Message;
            logger.LogWarning(
                "Unable to keep {Process} frameless: {Reason}",
                snapshot.ProcessName,
                result.Message);
        }
    }

    public void Dispose()
    {
        Exit();
        GC.SuppressFinalize(this);
    }
}
