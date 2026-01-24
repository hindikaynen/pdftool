using System;
using System.Drawing;
using System.IO;

namespace PdfTool.Tests;

public static class ImageComparer
{
    public static void AssertPngEqual(string expectedPath, string actualPath)
    {
        if (!File.Exists(expectedPath))
            throw new FileNotFoundException("Expected baseline is missing", expectedPath);
        if (!File.Exists(actualPath))
            throw new FileNotFoundException("Actual image is missing", actualPath);

        using var expected = (Bitmap)Image.FromFile(expectedPath);
        using var actual = (Bitmap)Image.FromFile(actualPath);

        if (expected.Width != actual.Width || expected.Height != actual.Height)
            throw new Exception($"Image size mismatch. Expected {expected.Width}x{expected.Height}, actual {actual.Width}x{actual.Height}");

        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                if (expected.GetPixel(x, y).ToArgb() != actual.GetPixel(x, y).ToArgb())
                    throw new Exception($"Pixel mismatch at ({x},{y})");
            }
        }
    }
}
