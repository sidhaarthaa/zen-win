using System.Drawing;

namespace ZenWin.Models;

public sealed record WindowSnapshot
{
    public required nint Handle { get; init; }
    public required uint ProcessId { get; init; }
    public required int Style { get; init; }
    public required int ExtendedStyle { get; init; }
    public required Rectangle Bounds { get; init; }
    public required WindowPlacementSnapshot Placement { get; init; }
    public required nint Menu { get; init; }
    public required bool WasMaximized { get; init; }
    public required bool WasMinimized { get; init; }
    public required string ProcessName { get; init; }
    public required string Title { get; init; }
}

public sealed record WindowPlacementSnapshot
{
    public required int Flags { get; init; }
    public required int ShowCommand { get; init; }
    public required Point MinPosition { get; init; }
    public required Point MaxPosition { get; init; }
    public required Rectangle NormalBounds { get; init; }
}
