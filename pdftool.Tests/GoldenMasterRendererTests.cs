using System;
using System.IO;
using NUnit.Framework;
using iText.Kernel.Pdf;

namespace PdfTool.Tests;

[TestFixture]
public class GoldenMasterRendererTests
{
    [Test]
    public void Case01_RectAndText_Page1() => RunSinglePage("case01.json", pageNumber1Based: 1);

    [Test]
    public void Case02_RectCornerRadius_Page1() => RunSinglePage("case02.json", pageNumber1Based: 1);

    private static void RunSinglePage(string specFileName, int pageNumber1Based)
    {
        // Expected baselines live in the source tree (pdftool.Tests/TestData/expected).
        // Actual artifacts are written into the test work directory (bin/...), so they don't pollute the repo.
        var testData = TestPaths.GetTestDataDir();

        var inputPdf = Path.Combine(testData, "input", "empty-10p.pdf");
        var specJson = Path.Combine(testData, "specs", specFileName);

        var workDir = TestContext.CurrentContext.WorkDirectory;
        var actualDir = Path.Combine(workDir, "pdftool_test_artifacts", "actual");
        Directory.CreateDirectory(actualDir);

        var expectedDir = Path.Combine(testData, "expected");
        Directory.CreateDirectory(expectedDir);

        // Baseline naming: {TestClass}.{TestMethod}[.{Case}].p{Page}.png
        var testId = $"{TestContext.CurrentContext.Test.ClassName}.{TestContext.CurrentContext.Test.MethodName}";

        var actualPdf = Path.Combine(actualDir, $"{testId}.output.pdf");

        // Apply
        using (var reader = new PdfReader(inputPdf))
        using (var writer = new PdfWriter(actualPdf))
        using (var pdf = new PdfDocument(reader, writer))
        {
            var spec = JsonSpecParser.Parse(specJson);
            PdfApplyEngine.Apply(pdf, spec);
        }

        // Rasterize page
        var dpi = 144;
        var actualPng = Path.Combine(actualDir, $"{testId}.p{pageNumber1Based}.png");
        PdfRasterizer.RenderPageToPng(actualPdf, pageNumber1Based: pageNumber1Based, dpi: dpi, pngPath: actualPng);

        var expectedPng = Path.Combine(expectedDir, $"{testId}.p{pageNumber1Based}.png");

        if (!File.Exists(expectedPng))
        {
            if (GoldenTestConfig.UpdateBaselines)
            {
                File.Copy(actualPng, expectedPng, overwrite: true);
                Assert.Pass("Baseline created in TestData/expected. Review and commit expected PNG.");
            }

            Assert.Fail("Baseline is missing. Enable UPDATE_BASELINES in GoldenTestConfig.cs to generate it.");
        }

        if (GoldenTestConfig.UpdateBaselines)
        {
            File.Copy(actualPng, expectedPng, overwrite: true);
            Assert.Pass("Baseline updated in TestData/expected. Review and commit expected PNG.");
        }

        ImageComparer.AssertPngEqual(expectedPng, actualPng);
    }
}
