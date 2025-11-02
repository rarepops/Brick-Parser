using System.Xml;

static int Usage()
{
    Console.Error.WriteLine("Usage: LxfmlSharp.Cli.exe <path-to-file.lxfml>");
    return 2;
}

static string ResolvePath(string[] args)
{
    if (args.Length > 0)
    {
        return args[0];
    }

    // IDE fallback: try repo-relative sample
    var candidates = new[]
    {
        Path.Combine(
            Environment.CurrentDirectory,
            @"Brick Parser\LxfmlSharp.Cli\Samples\everyoneisawesome.lxfml"
        ),
        Path.Combine(
            AppContext.BaseDirectory,
            @"Brick Parser\LxfmlSharp.Cli\Samples\everyoneisawesome.lxfml"
        ),
        Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, @"..\..\..\Samples\everyoneisawesome.lxfml")
        ),
    };

    foreach (var c in candidates)
    {
        if (File.Exists(c))
        {
            return c;
        }
    }

    Console.Error.WriteLine("No args provided and default sample not found at:");
    foreach (var c in candidates)
    {
        Console.Error.WriteLine($"- \"{c}\"");
    }

    Environment.Exit(Usage());
    return "";
}

var path = ResolvePath(args);
if (!File.Exists(path))
{
    Console.Error.WriteLine($"File not found: \"{path}\"");
    return 1;
}

try
{
    var model = LxfmlReader.Load(path); // see: LxfmlSharp.Core\LxfmlReader.cs
    Console.WriteLine($"Loading: \"{path}\"");
    Console.WriteLine($"Metadata: v{model.VersionMajor}.{model.VersionMinor}.{model.VersionPatch}");
    Console.WriteLine($"Bricks: {model.Bricks.Count}");
    var parts = model.Bricks.Sum(b => b.Parts.Count);
    Console.WriteLine($"Parts: {parts}");
    return 0;
}
catch (InvalidDataException ex)
{
    Console.Error.WriteLine($"Invalid LXFML: {ex.Message}");
    return 1;
}
catch (XmlException ex)
{
    Console.Error.WriteLine($"XML error: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Unexpected error: {ex.Message}");
    return 1;
}
