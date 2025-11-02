using System.Xml;
using LxfmlSharp.Core.Components;

namespace LxfmlSharp.Core;

public static class LxfmlReader
{
    public static Model Load(string filePath)
    {
        using var fs = File.OpenRead(filePath);
        using var xr = XmlReader.Create(
            fs,
            new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Ignore,
            }
        );
        return ReadModel(xr);
    }

    public static Model LoadFromString(string xml)
    {
        using var sr = new StringReader(xml);
        using var xr = XmlReader.Create(
            sr,
            new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Ignore,
            }
        );
        return ReadModel(xr);
    }

    private static Model ReadModel(XmlReader xr)
    {
        var model = new Model();

        while (xr.Read())
        {
            if (xr.NodeType == XmlNodeType.Element && Is(xr, "LXFML"))
            {
                try
                {
                    model = new Model
                    {
                        VersionMajor = GetInt(xr, "versionMajor"),
                        VersionMinor = GetInt(xr, "versionMinor"),
                        VersionPatch = GetInt(xr, "versionPatch"),
                    };
                }
                catch (InvalidDataException ex)
                {
                    throw new InvalidDataException($"Line {GetLineInfo(xr)}: {ex.Message}", ex);
                }

                if (xr.IsEmptyElement)
                {
                    return model;
                }

                break;
            }
        }

        // Within <LXFML> — scan for <Bricks>
        bool foundBricks = false;
        while (xr.Read())
        {
            if (xr.NodeType == XmlNodeType.Element && Is(xr, "Bricks"))
            {
                foundBricks = true;

                // If empty, early exit
                if (xr.IsEmptyElement)
                {
                    break;
                }

                // Inside <Bricks> — read <Brick> children
                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.EndElement && Is(xr, "Bricks"))
                    {
                        break;
                    }

                    if (xr.NodeType == XmlNodeType.Element && Is(xr, "Brick"))
                    {
                        var brick = ReadBrick(xr);
                        model.Bricks.Add(brick);
                    }
                }
            }
            else if (xr.NodeType == XmlNodeType.EndElement && Is(xr, "LXFML"))
            {
                break;
            }
        }

        // Validate xml structure
        if (model.VersionMajor == 0 && model.VersionMinor == 0 && model.VersionPatch == 0)
        {
            throw new InvalidDataException("Missing <LXFML> root or version attributes.");
        }

        if (!foundBricks)
        {
            throw new InvalidDataException("Missing <Bricks> container in <LXFML>.");
        }

        return model;
    }

    private static Brick ReadBrick(XmlReader xr)
    {
        var designIdAttr = GetString(xr, "designId");
        if (
            string.IsNullOrWhiteSpace(designIdAttr) || !int.TryParse(designIdAttr, out int designId)
        )
        {
            throw new InvalidDataException(
                $"Line {GetLineInfo(xr)}: Brick requires numeric designId, got '{designIdAttr}'"
            );
        }

        var brick = new Brick { Uuid = GetString(xr, "uuid"), DesignId = designId };

        // If empty, return
        if (xr.IsEmptyElement)
        {
            return brick;
        }

        while (xr.Read())
        {
            if (xr.NodeType == XmlNodeType.EndElement && Is(xr, "Brick"))
            {
                break;
            }

            if (xr.NodeType == XmlNodeType.Element && Is(xr, "Part"))
            {
                var part = ReadPart(xr);
                brick.Parts.Add(part);
            }
        }
        return brick;
    }

    private static Part ReadPart(XmlReader xr)
    {
        var partDesignAttr = GetString(xr, "designId");
        if (
            string.IsNullOrWhiteSpace(partDesignAttr)
            || !int.TryParse(partDesignAttr, out var partDesignId)
        )
        {
            throw new InvalidDataException(
                $"Line {GetLineInfo(xr)}: Part requires numeric designId, got '{partDesignAttr}'"
            );
        }

        var materialsRaw = GetString(xr, "materials");

        var part = new Part
        {
            Uuid = GetString(xr, "uuid"),
            PartType = GetString(xr, "type"),
            Materials = LxfmlParsers.ParseMaterials(materialsRaw),
            DesignId = partDesignId,
        };

        if (xr.IsEmptyElement)
        {
            return part;
        }

        while (xr.Read())
        {
            if (xr.NodeType == XmlNodeType.EndElement && Is(xr, "Part"))
            {
                break;
            }

            if (xr.NodeType == XmlNodeType.Element && Is(xr, "Bone"))
            {
                var bone = ReadBone(xr);
                part.Bones.Add(bone);
            }
        }

        return part;
    }

    private static Bone ReadBone(XmlReader xr)
    {
        var transformStr =
            GetString(xr, "transformation") ?? GetString(xr, "t") ?? GetString(xr, "transform");
        if (string.IsNullOrWhiteSpace(transformStr))
        {
            throw new InvalidDataException(
                $"Line {GetLineInfo(xr)}: Bone requires 'transformation' attribute with 12 values"
            );
        }

        float[]? arr;
        try
        {
            arr = LxfmlParsers.ParseTransform3x4(transformStr);
            if (arr?.Length != 12)
            {
                throw new InvalidDataException(
                    $"Bone transform expected 12 values, got {arr?.Length ?? 0}"
                );
            }
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidDataException($"Line {GetLineInfo(xr)}: {ex.Message}", ex);
        }

        var bone = new Bone { Uuid = GetString(xr, "uuid"), Transformation3x4 = arr };

        if (!xr.IsEmptyElement)
        {
            while (xr.Read())
            {
                if (xr.NodeType == XmlNodeType.EndElement && Is(xr, "Bone"))
                {
                    break;
                }
            }
        }

        if (!bone.IsValid)
        {
            throw new InvalidDataException(
                $"Line {GetLineInfo(xr)}: Bone transform contains non-finite values (NaN or Infinity)"
            );
        }

        return bone;
    }

    private static bool Is(XmlReader xr, string localName) =>
        xr.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase);

    private static string? GetString(XmlReader xr, string attr)
    {
        if (xr is null)
        {
            return null;
        }

        if (xr.HasAttributes)
        {
            for (int i = 0; i < xr.AttributeCount; i++)
            {
                xr.MoveToAttribute(i);
                if (xr.LocalName.Equals(attr, StringComparison.OrdinalIgnoreCase))
                {
                    var v = xr.Value;
                    xr.MoveToElement();
                    return string.IsNullOrWhiteSpace(v) ? null : v;
                }
            }
            xr.MoveToElement();
        }

        return null;
    }

    private static int GetInt(XmlReader xr, string attr, int defaultValue = 0)
    {
        var s = GetString(xr, attr);
        return int.TryParse(s, out var v) ? v : defaultValue;
    }

    private static string GetLineInfo(XmlReader xr)
    {
        if (xr is IXmlLineInfo li && li.HasLineInfo())
        {
            return $"{li.LineNumber},{li.LinePosition}";
        }

        return "unknown";
    }
}
