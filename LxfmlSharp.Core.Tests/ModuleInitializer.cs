using System.Runtime.CompilerServices;
using System.Text;

namespace LxfmlSharp.Core.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Configure Verify to use UTF-8 without BOM to match received files
        VerifierSettings.UseEncoding(new UTF8Encoding(false));

        // Optional: Configure Verify behavior
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.DontScrubGuids();
    }
}