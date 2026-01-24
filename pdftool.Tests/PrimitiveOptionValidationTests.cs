using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace PdfTool.Tests;

[TestFixture]
public class PrimitiveOptionValidationTests
{
    // ---------------- Rect ----------------

    [Test]
    public void Rect_Valid_Minimal_Passes()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "R",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "rect", "rect": [0,0,10,10] }
              ]
            }
          ]
        }
        """);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Rect_NegativeWidth_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "R",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "rect", "rect": [0,0,-10,10] }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Rect_NegativeHeight_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "R",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "rect", "rect": [0,0,10,-10] }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Rect_StrokeWidthNegative_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "R",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "rect", "rect": [0,0,10,10], "strokeWidth": -0.1 }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Rect_CornerRadiusNegative_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "R",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "rect", "rect": [0,0,10,10], "cornerRadius": -1 }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Rect_RectArrayWrongLen_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "R",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "rect", "rect": [0,0,10] }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    // ---------------- Line ----------------

    [Test]
    public void Line_Valid_Minimal_Passes()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "L",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "line", "from": [0,0], "to": [10,10] }
              ]
            }
          ]
        }
        """);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Line_FromWrongLen_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "L",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "line", "from": [0], "to": [10,10] }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Line_ToWrongLen_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "L",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "line", "from": [0,0], "to": [10] }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Line_StrokeWidthNegative_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "L",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "line", "from": [0,0], "to": [10,10], "strokeWidth": -1 }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    // ---------------- Text ----------------

    [Test]
    public void Text_At_Valid_Passes()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "at": [10,20], "value": "Hello" }
              ]
            }
          ]
        }
        """);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_Rect_Wrap_Valid_Passes()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "rect": [0,0,100,50], "wrap": true, "value": "Hello world" }
              ]
            }
          ]
        }
        """);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_MustHaveExactlyOneOfAtOrRect_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "value": "X" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_AtAndRectTogether_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "at": [0,0], "rect": [0,0,10,10], "value": "X" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_AtWrongLen_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "at": [0], "value": "X" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_RectWrongLen_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "rect": [0,0,10], "value": "X" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_RectNegativeWidth_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "rect": [0,0,-10,10], "value": "X" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_RectNegativeHeight_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "rect": [0,0,10,-10], "value": "X" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_SizeMustBePositive_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "at": [0,0], "size": 0, "value": "X" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_ValueMustBeNonEmpty_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "at": [0,0], "value": "" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Text_WrapTrueRequiresRect_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "T",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "at": [0,0], "wrap": true, "value": "X" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    // ---------------- Image ----------------

    [Test]
    public void Image_Valid_PngBase64_Passes()
    {
        // 1x1 transparent PNG
        const string png1x1 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PuL9xQAAAABJRU5ErkJggg==";

        var ov = ParseFirstOverlay($$"""
        {
          "overlays": [
            {
              "name": "I",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "image",
                  "rect": [0,0,10,10],
                  "base64": "{{png1x1}}"
                }
              ]
            }
          ]
        }
        """);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Image_RectWrongLen_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "I",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "image", "rect": [0,0,10], "base64": "AA==" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Image_NegativeWidthHeight_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "I",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "image", "rect": [0,0,-10,10], "base64": "AA==" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Image_DataMissing_Fails()
    {
        // NOTE: 'data' is required by the model, so this fails during JSON parsing (before validation).
        Assert.Throws<JsonException>(() => ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "I",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "image", "rect": [0,0,10,10] }
              ]
            }
          ]
        }
        """));
    }

    [Test]
    public void Image_MimeMissing_Fails()
    {
        // NOTE: 'mime' is required by the model, so this fails during JSON parsing (before validation).
        Assert.Throws<JsonException>(() => ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "I",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "image", "rect": [0,0,10,10], "base64": "AA==" }
              ]
            }
          ]
        }
        """));
    }

    [Test]
    public void Image_MimeUnsupported_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "I",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "image", "rect": [0,0,10,10], "base64": "AA==" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Image_Base64Missing_Fails()
    {
        // NOTE: 'base64' is required by the model, so this fails during JSON parsing (before validation).
        Assert.Throws<JsonException>(() => ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "I",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "image", "rect": [0,0,10,10], "base64": "!!!not-base64!!!" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    // ---------------- Barcode ----------------

    [Test]
    public void Barcode_Qr_Valid_NoOptions_Passes()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "barcode", "kind": "qr", "rect": [0,0,100,100], "value": "HELLO" }
              ]
            }
          ]
        }
        """);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Barcode_Code128_Valid_ShowTextBool_Passes()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "barcode",
                  "kind": "code128",
                  "rect": [0,0,200,60],
                  "value": "123456",
                  "options": { "showText": true }
                }
              ]
            }
          ]
        }
        """);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Barcode_RectWrongLen_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "barcode", "kind": "qr", "rect": [0,0,100], "value": "HELLO" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Barcode_NegativeWidthHeight_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "barcode", "kind": "qr", "rect": [0,0,-100,100], "value": "HELLO" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Barcode_ValueMustBeNonEmpty_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "barcode", "kind": "qr", "rect": [0,0,100,100], "value": "" }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Barcode_OptionsMustBeObject_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "barcode",
                  "kind": "qr",
                  "rect": [0,0,100,100],
                  "value": "HELLO",
                  "options": "oops"
                }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Barcode_Qr_EcLevelMustBeString_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "barcode",
                  "kind": "qr",
                  "rect": [0,0,100,100],
                  "value": "HELLO",
                  "options": { "ecLevel": 1 }
                }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Barcode_Qr_EcLevelMustBeOneOfAllowed_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "barcode",
                  "kind": "qr",
                  "rect": [0,0,100,100],
                  "value": "HELLO",
                  "options": { "ecLevel": "Z" }
                }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    [Test]
    public void Barcode_Code128_ShowTextMustBeBoolean_Fails()
    {
        var ov = ParseFirstOverlay("""
        {
          "overlays": [
            {
              "name": "B",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "barcode",
                  "kind": "code128",
                  "rect": [0,0,200,60],
                  "value": "123456",
                  "options": { "showText": "yes" }
                }
              ]
            }
          ]
        }
        """);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(ov));
    }

    // ---------------- Helpers ----------------

    private static OverlaySpec ParseFirstOverlay(string json)
    {
        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);
        return spec.Overlays[0];
    }

    private static string WriteTempJson(string json)
    {
        var dir = Path.Combine(Path.GetTempPath(), "pdftool_tests");
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, json, Encoding.UTF8);
        return path;
    }
}
