using System.Diagnostics;

namespace LxfmlSharp.Core.Components;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class Model
{
    public int VersionMajor { get; init; }
    public int VersionMinor { get; init; }
    public int VersionPatch { get; init; }
    public List<Brick> Bricks { get; } = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            var meta = $"v{VersionMajor}.{VersionMinor}.{VersionPatch}";
            return $"Metadata={meta}, Bricks={Bricks.Count}";
        }
    }
}
