using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PdfTool.Tests;

public static class ImageComparer
{
    /// <summary>
    /// —равнивает два PNG с допуском на небольшие различи€ (дл€ anti-aliasing и т.п.).
    /// </summary>
    public static void AssertPngEqualWithTolerance(string expectedPath, string actualPath, int pixelTolerance = 10)
    {
        if (!File.Exists(expectedPath))
            throw new FileNotFoundException("Expected baseline is missing", expectedPath);

        if (!File.Exists(actualPath))
            throw new FileNotFoundException("Actual image is missing", actualPath);

        using var expected = Image.Load<Rgba32>(expectedPath);
        using var actual = Image.Load<Rgba32>(actualPath);

        if (expected.Width != actual.Width || expected.Height != actual.Height)
            throw new Exception(
                $"Image size mismatch. Expected {expected.Width}x{expected.Height}, actual {actual.Width}x{actual.Height}");

        var expectedFrame = expected.Frames[0];
        var actualFrame = actual.Frames[0];

        int mismatchCount = 0;
        const int maxMismatches = 10; // ¬ыводим только первые 10 ошибок

        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                var exp = expectedFrame[x, y];
                var act = actualFrame[x, y];

                int rDiff = Math.Abs((int)exp.R - (int)act.R);
                int gDiff = Math.Abs((int)exp.G - (int)act.G);
                int bDiff = Math.Abs((int)exp.B - (int)act.B);
                int aDiff = Math.Abs((int)exp.A - (int)act.A);

                int maxDiff = Math.Max(Math.Max(rDiff, gDiff), Math.Max(bDiff, aDiff));

                if (maxDiff > pixelTolerance)
                {
                    if (mismatchCount < maxMismatches)
                    {
                        throw new Exception(
                            $"Pixel mismatch at ({x},{y}). " +
                            $"Expected RGBA({exp.R},{exp.G},{exp.B},{exp.A}), " +
                            $"actual RGBA({act.R},{act.G},{act.B},{act.A}), " +
                            $"max diff: {maxDiff}");
                    }

                    mismatchCount++;
                }
            }
        }

        if (mismatchCount > maxMismatches)
            throw new Exception($"Too many pixel mismatches ({mismatchCount} total, showing first {maxMismatches})");
    }
}
