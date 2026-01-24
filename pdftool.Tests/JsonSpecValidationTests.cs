using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace PdfTool.Tests;

[TestFixture]
public class JsonSpecValidationTests
{
    [Test]
    public void ParseAndValidate_MinimalCornerOverlay_Ok()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "all",
              "placement": { "mode": "corner", "corner": "topRight" },
              "primitives": [
                { "type": "text", "at": [0,0], "size": 10, "value": "X" }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.That(spec.Overlays, Has.Count.EqualTo(1));
        PagesRangeParser.ValidateSyntax(spec.Overlays[0].Pages);
        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePlacement_CornerRequiresCorner()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner" },
              "primitives": []
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePlacement_TextMarkerRequiresText()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "textMarker", "align": "bottomLeft" },
              "primitives": []
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePrimitive_RectCornerRadius_MustBeNonNegative()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "rect", "rect": [0,0,10,10], "cornerRadius": -1 }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePrimitive_ImageBase64_Valid()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "image",
                  "rect": [0,0,10,10],
                  "data": { "mime": "image/png", "base64": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/Pbqk0wAAAABJRU5ErkJggg==" }
                }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePrimitive_ImageBase64_Invalid_Fails()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "image",
                  "rect": [0,0,10,10],
                  "data": { "mime": "image/png", "base64": "not_base64!!!" }
                }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePrimitive_TextWrap_RequiresRect()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "at": [0,0], "wrap": true, "value": "Hello" }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePrimitive_TextBlock_WithWrap_Ok()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                { "type": "text", "rect": [0,0,100,50], "wrap": true, "value": "Hello world" }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePrimitive_BarcodeQr_Valid()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "barcode",
                  "kind": "qr",
                  "rect": [0,0,100,100],
                  "value": "https://example.com",
                  "options": { "ecLevel": "M" }
                }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.DoesNotThrow(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePrimitive_BarcodeQr_InvalidEcLevel_Fails()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "barcode",
                  "kind": "qr",
                  "rect": [0,0,100,100],
                  "value": "https://example.com",
                  "options": { "ecLevel": "Z" }
                }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }

    [Test]
    public void ValidatePrimitive_BarcodeCode128_ShowTextMustBeBool()
    {
        var json = """
        {
          "overlays": [
            {
              "name": "A",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft" },
              "primitives": [
                {
                  "type": "barcode",
                  "kind": "code128",
                  "rect": [0,0,200,60],
                  "value": "A123",
                  "options": { "showText": "yes" }
                }
              ]
            }
          ]
        }
        """;

        var path = WriteTempJson(json);
        var spec = JsonSpecParser.Parse(path);

        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }



[Test]
public void ValidatePrimitive_TextAtAndRectTogether_Fails()
{
    var json = """
    {
      "overlays": [
        {
          "name": "A",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topLeft" },
          "primitives": [
            { "type": "text", "at": [0,0], "rect": [0,0,10,10], "value": "Hello" }
          ]
        }
      ]
    }
    """;

    var path = WriteTempJson(json);
    var spec = JsonSpecParser.Parse(path);

    Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
}

[Test]
public void ValidatePrimitive_TextMissingAtAndRect_Fails()
{
    var json = """
    {
      "overlays": [
        {
          "name": "A",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topLeft" },
          "primitives": [
            { "type": "text", "value": "Hello" }
          ]
        }
      ]
    }
    """;

    var path = WriteTempJson(json);
    var spec = JsonSpecParser.Parse(path);

    Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
}

[Test]
public void Parse_ImageWithoutData_ShouldFailAtParseOrValidation()
{
    var json = """
    {
      "overlays": [
        {
          "name": "A",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topLeft" },
          "primitives": [
            { "type": "image", "rect": [0,0,10,10] }
          ]
        }
      ]
    }
    """;

    var path = WriteTempJson(json);

    try
    {
        var spec = JsonSpecParser.Parse(path);
        Assert.Throws<FormatException>(() => JsonSpecValidator.ValidateOverlay(spec.Overlays[0]));
    }
    catch (Exception ex)
    {
        // System.Text.Json may throw JsonException due to required members
        Assert.That(ex, Is.InstanceOf<System.Text.Json.JsonException>().Or.InstanceOf<FormatException>());
    }
}

[Test]
public void Parse_UnknownPrimitiveType_ShouldFailAtParse()
{
    var json = """
    {
      "overlays": [
        {
          "name": "A",
          "pages": "1",
          "placement": { "mode": "corner", "corner": "topLeft" },
          "primitives": [
            { "type": "unknownThing", "foo": 1 }
          ]
        }
      ]
    }
    """;

    var path = WriteTempJson(json);
    Assert.Throws<System.Text.Json.JsonException>(() => JsonSpecParser.Parse(path));
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
