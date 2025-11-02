using System.Diagnostics;

namespace LxfmlSharp.Core.Components;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class Part
{
    public int DesignId { get; init; }
    public string? Uuid { get; init; }
    public string? PartType { get; init; }
    public IReadOnlyList<Material> Materials { get; init; } = Array.Empty<Material>();
    public List<Bone> Bones { get; } = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay =>
        $"DesignId={DesignId}, Uuid={(string.IsNullOrEmpty(Uuid) ? "null" : Uuid)}, Type={PartType ?? "null"}, Materials={Materials?.Count ?? 0}, Bones={Bones.Count}";
}
