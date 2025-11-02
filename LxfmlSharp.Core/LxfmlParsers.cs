using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LxfmlSharp.Core.Components;

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

    public static double[] ParseTransform3x4(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return Array.Empty<double>();
        }

        var tokens = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var vals = new List<double>(12);
        foreach (var t in tokens)
        {
            if (
                double.TryParse(
                    t,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var d
                )
            )
            {
                vals.Add(d);
            }
        }

        if (vals.Count != 12)
        {
            throw new InvalidDataException($"Expected 12 transform values, got {vals.Count}.");
        }

        if (vals.Any(v => double.IsNaN(v) || double.IsInfinity(v)))
        {
            throw new InvalidDataException("Transform contains non-finite numbers.");
        }

        return vals.ToArray();
    }
}