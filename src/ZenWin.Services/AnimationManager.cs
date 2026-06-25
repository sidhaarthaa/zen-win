namespace ZenWin.Services;

public sealed class AnimationManager
{
    public TimeSpan StandardDuration { get; } = TimeSpan.FromMilliseconds(200);
    public TimeSpan FastDuration { get; } = TimeSpan.FromMilliseconds(120);

    public double EaseOutCubic(double progress)
    {
        var p = Math.Clamp(progress, 0, 1);
        return 1 - Math.Pow(1 - p, 3);
    }
}
