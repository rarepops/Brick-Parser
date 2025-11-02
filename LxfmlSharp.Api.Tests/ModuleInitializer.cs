using System.Runtime.CompilerServices;
using System.Text;

namespace LxfmlSharp.Api.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.UseEncoding(new UTF8Encoding(false));
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.DontScrubGuids();
    }
}
