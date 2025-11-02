using System.Diagnostics;

namespace LxfmlSharp.Core.Components;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly record struct Material(int ColorId, int? Variant)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay =>
        $"ColorId={ColorId}, Variant={(Variant.HasValue ? Variant.Value.ToString() : "null")}";
}
