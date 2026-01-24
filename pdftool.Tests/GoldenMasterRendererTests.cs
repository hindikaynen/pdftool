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



[Test]
// Primitive: image (base64 PNG) in rect (fit=contain) at TOP-LEFT
public void Case20_ImageRect_PngBase64_Page1()
{
    const string png1x1 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PuL9xQAAAABJRU5ErkJggg==";

    var json = """
    {
      "overlays": [
        {
          "name": "case20",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topLeft", "offset": [72,72] },
          "primitives": [
            { "type": "rect", "rect": [0,0,220,90], "cornerRadius": 10, "strokeWidth": 1 },
            { "type": "image", "rect": [12,12,66,66], "base64": "__PNG__" },
            { "type": "text", "at": [90,40], "size": 11, "value": "PNG base64 image" }
          ]
        }
      ]
    }
    """.Replace("__PNG__", png1x1);

    RunSinglePage(json);
}




[Test]
// Primitive: barcode.qr with EC level L, placed at TOP-RIGHT
public void Case23_BarcodeQr_EcL_TopRight_Page1()
{
    var json = """
    {
      "overlays": [
        {
          "name": "case23",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topRight", "offset": [72,72] },
          "primitives": [
            { "type": "rect", "rect": [0,0,240,110], "cornerRadius": 10, "strokeWidth": 1 },
            { "type": "barcode", "kind": "qr", "rect": [12,12,86,86], "value": "HELLO-QR-L", "options": { "ecLevel": "L" } },
            { "type": "text", "at": [110,58], "size": 11, "value": "QR EC=L" }
          ]
        }
      ]
    }
    """;

    RunSinglePage(json);
}

[Test]
// Primitive: barcode.qr with EC level Q, placed at BOTTOM-LEFT
public void Case24_BarcodeQr_EcQ_BottomLeft_Page1()
{
    var json = """
    {
      "overlays": [
        {
          "name": "case24",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "bottomLeft", "offset": [72,72] },
          "primitives": [
            { "type": "rect", "rect": [0,0,240,110], "cornerRadius": 10, "strokeWidth": 1 },
            { "type": "barcode", "kind": "qr", "rect": [12,12,86,86], "value": "HELLO-QR-Q", "options": { "ecLevel": "Q" } },
            { "type": "text", "at": [110,58], "size": 11, "value": "QR EC=Q" }
          ]
        }
      ]
    }
    """;

    RunSinglePage(json);
}

[Test]
// Primitive: barcode.code128 with human-readable text ON, placed at BOTTOM-RIGHT
public void Case25_BarcodeCode128_ShowText_BottomRight_Page1()
{
    var json = """
    {
      "overlays": [
        {
          "name": "case25",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "bottomRight", "offset": [72,72] },
          "primitives": [
            { "type": "rect", "rect": [0,0,340,120], "cornerRadius": 10, "strokeWidth": 1 },
            { "type": "barcode", "kind": "code128", "rect": [12,50,316,55], "value": "ABC-123-XYZ", "options": { "showText": true } },
            { "type": "text", "at": [12,18], "size": 11, "value": "Code128 showText=true" }
          ]
        }
      ]
    }
    """;

    RunSinglePage(json);
}

[Test]
// Primitive: barcode.code128 narrow rect (stress fit into small width), showText OFF
public void Case26_BarcodeCode128_Narrow_NoText_Page1()
{
    var json = """
    {
      "overlays": [
        {
          "name": "case26",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topLeft", "offset": [72,150] },
          "primitives": [
            { "type": "rect", "rect": [0,0,220,105], "cornerRadius": 10, "strokeWidth": 1 },
            { "type": "barcode", "kind": "code128", "rect": [12,38,196,55], "value": "01234567890123456789", "options": { "showText": false } },
            { "type": "text", "at": [12,14], "size": 11, "value": "Code128 narrow (no text)" }
          ]
        }
      ]
    }
    """;

    RunSinglePage(json);
}

[Test]
// Primitive: multiple barcodes in a single overlay (QR + Code128)
public void Case27_Barcode_Multiple_Primitives_Page1()
{
    var json = """
    {
      "overlays": [
        {
          "name": "case27",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topRight", "offset": [72,150] },
          "primitives": [
            { "type": "rect", "rect": [0,0,420,140], "cornerRadius": 10, "strokeWidth": 1 },

            { "type": "barcode", "kind": "qr", "rect": [12,38,90,90], "value": "MULTI-QR", "options": { "ecLevel": "M" } },
            { "type": "text", "at": [12,14], "size": 11, "value": "QR (M)" },

            { "type": "barcode", "kind": "code128", "rect": [120,55,288,60], "value": "MULTI-128", "options": { "showText": false } },
            { "type": "text", "at": [120,14], "size": 11, "value": "Code128 (no text)" }
          ]
        }
      ]
    }
    """;

    RunSinglePage(json);
}
    

[Test]
// Primitive: QR code default EC level (M), medium size
public void Case23_QR_DefaultEc_Page1()
{
    RunSinglePage("""
    {
      "overlays": [
        {
          "name": "qr-default",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "bottomLeft", "offset": [72,72] },
          "primitives": [
            { "type": "barcode", "kind": "qr", "rect": [0,0,120,120], "value": "HELLO-QR" }
          ]
        }
      ]
    }
    """);
}

[Test]
// Primitive: QR code high EC level (H), large size
public void Case24_QR_EcH_Large_Page1()
{
    RunSinglePage("""
    {
      "overlays": [
        {
          "name": "qr-ech",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "bottomRight", "offset": [72,72] },
          "primitives": [
            { "type": "barcode", "kind": "qr", "rect": [0,0,160,160], "value": "HELLO-QR-H", "options": { "ecLevel": "H" } }
          ]
        }
      ]
    }
    """);
}

[Test]
// Primitive: Code128 barcode with human-readable text
public void Case25_Code128_WithText_Page1()
{
    RunSinglePage("""
    {
      "overlays": [
        {
          "name": "code128-text",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topRight", "offset": [72,72] },
          "primitives": [
            { "type": "barcode", "kind": "code128", "rect": [0,0,260,80], "value": "ABC-123-XYZ", "options": { "showText": true } }
          ]
        }
      ]
    }
    """);
}

[Test]
// Primitive: Code128 barcode without human-readable text
public void Case26_Code128_NoText_Wide_Page1()
{
    RunSinglePage("""
    {
      "overlays": [
        {
          "name": "code128-notext",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topLeft", "offset": [72,120] },
          "primitives": [
            { "type": "barcode", "kind": "code128", "rect": [0,0,320,60], "value": "NO-TEXT-128", "options": { "showText": false } }
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
