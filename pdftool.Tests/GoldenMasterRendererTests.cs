using System;
using System.IO;
using NUnit.Framework;
using iText.Kernel.Pdf;

namespace PdfTool.Tests;

[TestFixture]
public class GoldenMasterRendererTests
{
    [Test]
    public void Case01_RectAndText_Page1()
    {
        var baseDir = TestContext.CurrentContext.TestDirectory;
        var testData = Path.Combine(baseDir, "TestData");

        var inputPdf = Path.Combine(testData, "input", "empty-10p.pdf");
        var specJson = Path.Combine(testData, "specs", "case01.json");

        var actualDir = Path.Combine(testData, "actual");
        var expectedDir = Path.Combine(testData, "expected");

        Directory.CreateDirectory(actualDir);
        Directory.CreateDirectory(expectedDir);

        var actualPdf = Path.Combine(actualDir, "case01.output.pdf");

        // Apply
        using (var reader = new PdfReader(inputPdf))
        using (var writer = new PdfWriter(actualPdf))
        using (var pdf = new PdfDocument(reader, writer))
        {
            var spec = JsonSpecParser.Parse(specJson);
            PdfApplyEngine.Apply(pdf, spec);
        }

        // Rasterize page 1
        var dpi = 144;
        var actualPng = Path.Combine(actualDir, "case01.p1.png");
        PdfRasterizer.RenderPageToPng(actualPdf, pageNumber1Based: 1, dpi: dpi, pngPath: actualPng);

        var expectedPng = Path.Combine(expectedDir, "case01.p1.png");

        if (!File.Exists(expectedPng))
        {
            if (GoldenTestConfig.UpdateBaselines)
            {
                File.Copy(actualPng, expectedPng, overwrite: true);
                Assert.Pass("Baseline created. Review and commit expected PNG.");
            }

            Assert.Fail("Baseline is missing. Enable UPDATE_BASELINES in GoldenTestConfig.cs to generate it.");
        }

        if (GoldenTestConfig.UpdateBaselines)
        {
            File.Copy(actualPng, expectedPng, overwrite: true);
            Assert.Pass("Baseline updated. Review and commit expected PNG.");
        }

        ImageComparer.AssertPngEqual(expectedPng, actualPng);
    }
}
