using System;
using System.IO;
using NUnit.Framework;

namespace PdfTool.Tests;

public static class TestPaths
{
    /// <summary>
    /// Finds the directory that contains the specified marker file by walking up from the current test directory.
    /// Typically used to find the test project folder, so we can write baselines into the source tree (not bin/).
    /// </summary>
    public static string FindUpwards(string markerFileName)
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, markerFileName);
            if (File.Exists(candidate))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException($"Could not find '{markerFileName}' by walking up from '{TestContext.CurrentContext.TestDirectory}'.");
    }

    public static string GetTestProjectDir() => FindUpwards("pdftool.Tests.csproj");

    public static string GetTestDataDir() => Path.Combine(GetTestProjectDir(), "TestData");
}
