namespace ZenWin.Models;

public sealed record FramelessResult(bool Succeeded, string Message)
{
    public static FramelessResult Success(string? message = null) =>
        new(true, message ?? "The standard Windows frame was removed.");

    public static FramelessResult Failure(string message) =>
        new(false, message);
}
