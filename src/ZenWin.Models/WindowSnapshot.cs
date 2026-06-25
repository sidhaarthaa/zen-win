using System.Drawing;

namespace ZenWin.Models;

public sealed record WindowSnapshot
{
    public required nint Handle { get; init; }
    public required int Style { get; init; }
    public required int ExtendedStyle { get; init; }
    public required Rectangle Bounds { get; init; }
    public required bool WasMaximized { get; init; }
    public required bool WasMinimized { get; init; }
    public required nint InsertAfter { get; init; }
    public required string ProcessName { get; init; }
    public required string Title { get; init; }
}
