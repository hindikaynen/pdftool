using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using iText.Kernel.Pdf;

namespace PdfTool.Tests;

[TestFixture]
public class GoldenMasterRendererTests
{
    // NOTE:
    // Each test contains its JSON spec inline for maximum readability.
    // Baseline naming:
    // {TestClass}.{TestMethod}.p{Page}.png
    //
    // Corner placement uses Variant B semantics:
    // offset is ALWAYS "inward" from the chosen corner and SHOULD be small (<= 200pt):
    // - topLeft:     [right, down]
    // - topRight:    [left,  down]
    // - bottomLeft:  [right, up]
    // - bottomRight: [left,  up]

    [Test]
    // Primitive: rect (stroke) + text(at) at TOP-LEFT
    public void Case01_RectAndTextAt_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case01",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft", "offset": [72,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,240,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rect + text(at)" }
              ]
            }
          ]
        }
        """);
    }

    [Test]
    // Primitive: rect with cornerRadius + text(at) at TOP-RIGHT
    public void Case02_RectCornerRadius_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case02",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topRight", "offset": [72,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,240,60], "cornerRadius": 12, "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "cornerRadius=12" }
              ]
            }
          ]
        }
        """);
    }

    [Test]
    // Primitive: text(rect) with wrap=true at BOTTOM-LEFT
    public void Case03_TextRect_Wrap_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case03",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "bottomLeft", "offset": [72,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,320,90], "cornerRadius": 8, "strokeWidth": 1 },
                {
                  "type": "text",
                  "rect": [10,10,300,70],
                  "wrap": true,
                  "size": 11,
                  "value": "This is a long text that should wrap automatically inside a fixed rectangle."
                }
              ]
            }
          ]
        }
        """);
    }

    [Test]
    // Primitive: line with strokeWidth at BOTTOM-RIGHT
    public void Case04_Line_StrokeWidth_Page1()
    {
        RunSinglePage("""
          {
            "overlays": [
              {
                "name": "case04",
                "pages": "1",
                "placement": { "mode": "corner", "corner": "bottomRight", "offset": [72,72] },
                "primitives": [
                  { "type": "line", "from": [0,0], "to": [260,0], "strokeWidth": 6 },
                  { "type": "text", "at": [0,14], "size": 11, "value": "strokeWidth = 6pt" }
                ]
              }
            ]
          }
          """);
    }

    [Test]
    // Primitive: text(rect) with valign=top at TOP-LEFT
    public void Case06_TextRect_VAlignTop_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case06",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft", "offset": [72,140] },
              "primitives": [
                { "type": "rect", "rect": [0,0,320,110], "cornerRadius": 8, "strokeWidth": 1 },
                {
                  "type": "text",
                  "rect": [10,10,300,90],
                  "wrap": true,
                  "size": 12,
                  "valign": "top",
                  "value": "VAlign TOP\nLine 2\nLine 3"
                }
              ]
            }
          ]
        }
        """);
    }

    [Test]
    // Primitive: text(rect) with valign=middle at TOP-RIGHT
    public void Case07_TextRect_VAlignMiddle_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case07",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topRight", "offset": [72,140] },
              "primitives": [
                { "type": "rect", "rect": [0,0,320,110], "cornerRadius": 8, "strokeWidth": 1 },
                {
                  "type": "text",
                  "rect": [10,10,300,90],
                  "wrap": true,
                  "size": 12,
                  "valign": "middle",
                  "value": "VAlign MIDDLE\nLine 2\nLine 3"
                }
              ]
            }
          ]
        }
        """);
    }

    [Test]
    // Primitive: text(rect) with valign=bottom at BOTTOM-LEFT
    public void Case08_TextRect_VAlignBottom_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case08",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "bottomLeft", "offset": [72,140] },
              "primitives": [
                { "type": "rect", "rect": [0,0,320,110], "cornerRadius": 8, "strokeWidth": 1 },
                {
                  "type": "text",
                  "rect": [10,10,300,90],
                  "wrap": true,
                  "size": 12,
                  "valign": "bottom",
                  "value": "VAlign BOTTOM\nLine 2\nLine 3"
                }
              ]
            }
          ]
        }
        """);
    }

    [Test]
    // Primitive: text(rect) with halign=left at TOP-LEFT
    public void Case12_TextRect_HAlignLeft_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case12",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft", "offset": [72,200] },
              "primitives": [
                { "type": "rect", "rect": [0,0,320,90], "cornerRadius": 8, "strokeWidth": 1 },
                { "type": "text", "rect": [10,10,300,70], "wrap": false, "size": 18, "halign": "left", "valign": "middle", "value": "LEFT" }
              ]
            }
          ]
        }
        """);
    }

    [Test]
    // Primitive: text(rect) with halign=center at TOP-RIGHT
    public void Case13_TextRect_HAlignCenter_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case13",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topRight", "offset": [72,200] },
              "primitives": [
                { "type": "rect", "rect": [0,0,320,90], "cornerRadius": 8, "strokeWidth": 1 },
                { "type": "text", "rect": [10,10,300,70], "wrap": false, "size": 18, "halign": "center", "valign": "middle", "value": "CENTER" }
              ]
            }
          ]
        }
        """);
    }

    [Test]
    // Primitive: text(rect) with halign=right at BOTTOM-RIGHT
    public void Case14_TextRect_HAlignRight_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case14",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "bottomRight", "offset": [72,140] },
              "primitives": [
                { "type": "rect", "rect": [0,0,320,90], "cornerRadius": 8, "strokeWidth": 1 },
                { "type": "text", "rect": [10,10,300,70], "wrap": false, "size": 18, "halign": "right", "valign": "middle", "value": "RIGHT" }
              ]
            }
          ]
        }
        """);
    }

    private static void RunSinglePage(string jsonSpec)
    {
        var testData = TestPaths.GetTestDataDir();
        var inputPdf = Path.Combine(testData, "input", "empty-10p.pdf");

        var workDir = TestContext.CurrentContext.WorkDirectory;
        var actualDir = Path.Combine(workDir, "pdftool_test_artifacts", "actual");
        Directory.CreateDirectory(actualDir);

        var expectedDir = Path.Combine(testData, "expected");
        Directory.CreateDirectory(expectedDir);

        var testId = $"{TestContext.CurrentContext.Test.ClassName}.{TestContext.CurrentContext.Test.MethodName}";
        var actualPdf = Path.Combine(actualDir, $"{testId}.output.pdf");

        var specPath = WriteTempJson(jsonSpec);

        using (var reader = new PdfReader(inputPdf))
        using (var writer = new PdfWriter(actualPdf))
        using (var pdf = new PdfDocument(reader, writer))
        {
            var spec = JsonSpecParser.Parse(specPath);
            PdfApplyEngine.Apply(pdf, spec);
        }

        var actualPng = Path.Combine(actualDir, $"{testId}.p1.png");
        PdfRasterizer.RenderPageToPng(actualPdf, pageNumber1Based: 1, dpi: 144, pngPath: actualPng);

        var expectedPng = Path.Combine(expectedDir, $"{testId}.p1.png");

        if (!File.Exists(expectedPng))
        {
            if (GoldenTestConfig.UpdateBaselines)
            {
                File.Copy(actualPng, expectedPng, overwrite: true);
                Assert.Pass("Baseline created.");
            }

            Assert.Fail("Baseline missing. Enable UPDATE_BASELINES.");
        }

        if (GoldenTestConfig.UpdateBaselines)
        {
            File.Copy(actualPng, expectedPng, overwrite: true);
            Assert.Pass("Baseline updated.");
        }

        ImageComparer.AssertPngEqual(expectedPng, actualPng);
    }

    private static string WriteTempJson(string json)
    {
        var dir = Path.Combine(Path.GetTempPath(), "pdftool_golden_specs");
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, json, Encoding.UTF8);
        return path;
    }
}
