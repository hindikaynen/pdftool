//#define UPDATE_BASELINES
namespace PdfTool.Tests;

public static class GoldenTestConfig
{
    public const bool UpdateBaselines =
#if UPDATE_BASELINES
        true;
#else
        false;
#endif
}
