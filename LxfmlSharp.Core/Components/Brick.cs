using System.Diagnostics;

namespace LxfmlSharp.Core.Components;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class Brick
{
    public int DesignId { get; init; }
    public string? Uuid { get; init; }
    public List<Part> Parts { get; } = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay =>
        $"DesignId={DesignId}, Uuid={(string.IsNullOrEmpty(Uuid) ? "null" : Uuid)}, Parts={Parts.Count}";
}
