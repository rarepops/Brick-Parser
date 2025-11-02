using System.Diagnostics;

namespace LxfmlSharp.Core.Components;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class Bone
{
    public string? Uuid { get; init; }

    // 3x4 matrix row-major: [r00,r01,r02,tx, r10,r11,r12,ty, r20,r21,r22,tz]
    public required double[] Transformation3x4 { get; init; }

    public bool IsValid =>
        Transformation3x4 is { Length: 12 }
        && Array.TrueForAll(Transformation3x4, d => !double.IsNaN(d) && !double.IsInfinity(d));

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay =>
        $"Uuid={(string.IsNullOrEmpty(Uuid) ? "null" : Uuid)}, Valid={IsValid}, TransformLen={Transformation3x4?.Length ?? 0}";
}
