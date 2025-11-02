using System.Globalization;
using LxfmlSharp.Core.Components;

namespace LxfmlSharp.Core;

public static class LxfmlParsers
{
    public static IReadOnlyList<Material> ParseMaterials(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<Material>();
        }

        var list = new List<Material>();
        var tokens = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var tok in tokens)
        {
            var parts = tok.Split(':');
            if (parts.Length == 1 && int.TryParse(parts[0], out var color))
            {
                list.Add(new Material(color, null));
            }
            else if (
                parts.Length == 2
                && int.TryParse(parts[0], out var color2)
                && int.TryParse(parts[1], out var variant)
            )
            {
                list.Add(new Material(color2, variant));
            }
        }

        return list;
    }

    public static float[] ParseTransform3x4(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return Array.Empty<float>();
        }

        var tokens = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var vals = new List<float>(12);
        foreach (var t in tokens)
        {
            if (float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            {
                vals.Add(d);
            }
        }

        if (vals.Count != 12)
        {
            throw new InvalidDataException($"Expected 12 transform values, got {vals.Count}.");
        }

        if (vals.Any(v => float.IsNaN(v) || float.IsInfinity(v)))
        {
            throw new InvalidDataException("Transform contains non-finite numbers.");
        }

        return vals.ToArray();
    }
}
