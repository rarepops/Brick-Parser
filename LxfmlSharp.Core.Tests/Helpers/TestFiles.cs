namespace LxfmlSharp.Core.Tests.Helpers;

public static class TestFiles
{
    public static string ReadSample(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", name);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Test sample not found: {path}", path);
        }

        return File.ReadAllText(path);
    }
}
