using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using iText.Kernel.Pdf;
using Xunit;

namespace PdfTool.Tests;

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

    [Fact]
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
                { "type": "rect", "rect": [0,0,240,60], "stroke": "#ff0000ff", "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "font": "Times-Roman", "color": "#0000ffff", "value": "Rect + text(at)" }
              ]
            }
          ]
        }
        """);
    }

    [Fact]
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
                { "type": "rect", "rect": [0,0,240,60], "cornerRadius": 12, "fill": "#00ff0020", "stroke": "#00ff00ff", "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "font": "Helvetica-Bold", "color": "#00aa00ff", "value": "cornerRadius=12" }
              ]
            }
          ]
        }
        """);
    }

    [Fact]
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
                { "type": "rect", "rect": [0,0,320,90], "cornerRadius": 8, "fill": "#ffff0020", "stroke": "#000000ff", "strokeWidth": 1 },
                {
                  "type": "text",
                  "rect": [10,10,300,70],
                  "wrap": true,
                  "size": 11,
                  "font": "Courier",
                  "color": "#000000ff",
                  "value": "This is a long text that should wrap automatically inside a fixed rectangle."
                }
              ]
            }
          ]
        }
        """);
    }

    [Fact]
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
                { "type": "rect", "rect": [0,0,320,110], "cornerRadius": 8, "fill": "#00ffffff", "stroke": "#0000ffff", "strokeWidth": 1 },
                {
                  "type": "text",
                  "rect": [10,10,300,90],
                  "wrap": true,
                  "size": 12,
                  "valign": "top",
                  "font": "Times-Italic",
                  "color": "#0000ffff",
                  "value": "VAlign TOP\nLine 2\nLine 3"
                }
              ]
            }
          ]
        }
        """);
    }

    [Fact]
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
                { "type": "rect", "rect": [0,0,320,110], "cornerRadius": 8, "fill": "#ff00ff20", "stroke": "#ff00ffff", "strokeWidth": 1 },
                {
                  "type": "text",
                  "rect": [10,10,300,90],
                  "wrap": true,
                  "size": 12,
                  "valign": "middle",
                  "font": "Courier-Bold",
                  "color": "#ff00ffff",
                  "value": "VAlign MIDDLE\nLine 2\nLine 3"
                }
              ]
            }
          ]
        }
        """);
    }

    [Fact]
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
                { "type": "rect", "rect": [0,0,320,110], "cornerRadius": 8, "fill": "#ff000020", "stroke": "#ff0000ff", "strokeWidth": 1 },
                {
                  "type": "text",
                  "rect": [10,10,300,90],
                  "wrap": true,
                  "size": 12,
                  "valign": "bottom",
                  "font": "Helvetica-Oblique",
                  "color": "#ff0000ff",
                  "value": "VAlign BOTTOM\nLine 2\nLine 3"
                }
              ]
            }
          ]
        }
        """);
    }

    [Fact]
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
                { "type": "line", "from": [0,0], "to": [260,0], "stroke": "#ff8800ff", "strokeWidth": 6 },
                { "type": "text", "at": [0,14], "size": 11, "color": "#ff8800ff", "value": "strokeWidth = 6pt" }
              ]
            }
          ]
        }
        """);
    }

    [Fact]
    // Primitive: ellipse (fill+stroke) at TOP-LEFT
    public void Case05_Ellipse_FillStroke_Page1()
    {
        RunSinglePage("""
        {
          "overlays": [
            {
              "name": "case05",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft", "offset": [72,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,110], "cornerRadius": 10, "fill": "#00000010", "stroke": "#222222ff", "strokeWidth": 1 },
                { "type": "ellipse", "rect": [12,12,86,86], "fill": "#00ffffff", "stroke": "#ff00ffff", "strokeWidth": 2 },
                { "type": "text", "at": [110,58], "size": 11, "color": "#222222ff", "value": "Ellipse fill+stroke" }
              ]
            }
          ]
        }
        """);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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



[Fact]
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
            { "type": "rect", "rect": [0,0,220,90], "cornerRadius": 10, "fill": "#00000010", "stroke": "#222222ff", "strokeWidth": 1 },
            { "type": "image", "rect": [12,12,66,66], "fill": "#00ffffff", "stroke": "#ff00ffff", "strokeWidth": 2, "base64": "__PNG__" },
            { "type": "text", "at": [90,40], "size": 11, "color": "#222222ff", "value": "PNG base64 image" }
          ]
        }
      ]
    }
    """.Replace("__PNG__", png1x1);

    RunSinglePage(json);
}




[Fact]
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
            { "type": "rect", "rect": [0,0,240,110], "cornerRadius": 10, "fill": "#ffffffcc", "stroke": "#000000ff", "strokeWidth": 1 },
            { "type": "barcode", "kind": "qr", "rect": [12,12,86,86], "fill": "#ffffffff", "stroke": "#00000040", "strokeWidth": 1, "color": "#0000ffff", "value": "HELLO-QR-L", "options": { "ecLevel": "L" } },
            { "type": "text", "at": [110,58], "size": 11, "color": "#0000ffff", "value": "QR EC=L" }
          ]
        }
      ]
    }
    """;

    RunSinglePage(json);
}

[Fact]
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
            { "type": "rect", "rect": [0,0,240,110], "cornerRadius": 10, "fill": "#00ff0020", "stroke": "#00ff00ff", "strokeWidth": 1 },
            { "type": "barcode", "kind": "qr", "rect": [12,12,86,86], "fill": "#ffffffff", "stroke": "#00ff00ff", "strokeWidth": 1, "color": "#00aa00ff", "value": "HELLO-QR-Q", "options": { "ecLevel": "Q" } },
            { "type": "text", "at": [110,58], "size": 11, "color": "#00aa00ff", "value": "QR EC=Q" }
          ]
        }
      ]
    }
    """;

    RunSinglePage(json);
}

[Fact]
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
            { "type": "rect", "rect": [0,0,340,120], "cornerRadius": 10, "fill": "#0000ff10", "stroke": "#0000ffff", "strokeWidth": 1 },
            { "type": "barcode", "kind": "code128", "rect": [12,50,316,55], "fill": "#ffffffff", "stroke": "#0000ffff", "strokeWidth": 1, "color": "#0000ffff", "value": "ABC-123-XYZ", "options": { "showText": true } },
            { "type": "text", "at": [12,18], "size": 11, "color": "#0000ffff", "value": "Code128 showText=true" }
          ]
        }
      ]
    }
    """;

    RunSinglePage(json);
}

[Fact]
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

[Fact]
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
    

[Fact]
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

[Fact]
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

[Fact]
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

[Fact]
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

    [Fact]
    // Placement: textAnchor (occurrence=first) on page 1 + visual redaction (marker should disappear)
    public void Case30_TextAnchor_First_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case30",
              "pages": "1",
              "placement": { "mode": "textAnchor", "text": "<<ANCHOR_ONE>>", "occurrence": "first", "offset": [200,0] },
              "primitives": [
                { "type": "rect", "rect": [0,0,220,50], "strokeWidth": 1 },
                { "type": "text", "at": [10,18], "size": 12, "value": "textAnchor first" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "textanchor-2p.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Placement: textAnchor (occurrence=last) on page 2
    public void Case31_TextAnchor_Last_Page2()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case31",
              "pages": "2",
              "placement": { "mode": "textAnchor", "text": "<<ANCHOR_TWO>>", "occurrence": "last", "offset": [0,0] },
              "primitives": [
                { "type": "rect", "rect": [0,0,220,50], "strokeWidth": 1 },
                { "type": "text", "at": [10,18], "size": 12, "value": "textAnchor last" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "textanchor-2p.pdf",
        pageNumber1BasedToRender: 2);
    }

    [Fact]
    // Placement: textAnchor (occurrence=all) on page 2 (overlay rendered twice)
    public void Case32_TextAnchor_All_Page2()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case32",
              "pages": "2",
              "placement": { "mode": "textAnchor", "text": "<<ANCHOR_TWO>>", "occurrence": "all", "offset": [0,0] },
              "primitives": [
                { "type": "rect", "rect": [0,0,220,50], "strokeWidth": 1 },
                { "type": "text", "at": [10,18], "size": 12, "value": "textAnchor all" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "textanchor-2p.pdf",
        pageNumber1BasedToRender: 2);
    }

    [Fact]
    // Rotated page (/Rotate=90): corner placement TOP-LEFT
    public void Case40_Rotate90_Corner_TopLeft_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case40",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate90 corner TOP-LEFT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r90.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Rotated page (/Rotate=90): corner placement TOP-RIGHT
    public void Case41_Rotate90_Corner_TopRight_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case41",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topRight", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate90 corner TOP-RIGHT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r90.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Rotated page (/Rotate=90): corner placement BOTTOM-LEFT
    public void Case42_Rotate90_Corner_BottomLeft_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case42",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "bottomLeft", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate90 corner BOTTOM-LEFT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r90.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Rotated page (/Rotate=90): corner placement BOTTOM-RIGHT
    public void Case43_Rotate90_Corner_BottomRight_Page1()
    {
        RunOnePageFromInput(
            jsonSpec: """
                      {
                        "overlays": [
                          {
                            "name": "case43",
                            "pages": "1",
                            "placement": { "mode": "corner", "corner": "bottomRight", "offset": [150,72] },
                            "primitives": [
                              { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                              { "type": "text", "at": [12,22], "size": 12, "value": "Rotate90 corner BOTTOM-RIGHT" }
                            ]
                          }
                        ]
                      }
                      """,
            inputPdfFileName: "rot-empty-1p_r90.pdf",
            pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Rotated page (/Rotate=180): corner placement BOTTOM-RIGHT
    public void Case43_Rotate180_Corner_BottomRight_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case43",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "bottomRight", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate180 corner BOTTOM-RIGHT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r180.pdf",
        pageNumber1BasedToRender: 1);
    }

        [Fact]
    // Rotated page (/Rotate=180): corner placement TOP-LEFT
    public void Case40_Rotate180_Corner_TopLeft_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case40",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate180 corner TOP-LEFT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r180.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Rotated page (/Rotate=180): corner placement TOP-RIGHT
    public void Case41_Rotate180_Corner_TopRight_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case41",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topRight", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate180 corner TOP-RIGHT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r180.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Rotated page (/Rotate=180): corner placement BOTTOM-LEFT
    public void Case42_Rotate180_Corner_BottomLeft_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case42",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "bottomLeft", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate180 corner BOTTOM-LEFT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r180.pdf",
        pageNumber1BasedToRender: 1);
    }

        [Fact]
    // Rotated page (/Rotate=270): corner placement BOTTOM-RIGHT
    public void Case43_Rotate270_Corner_BottomRight_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case43",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "bottomRight", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate270 corner BOTTOM-RIGHT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r270.pdf",
        pageNumber1BasedToRender: 1);
    }

        [Fact]
    // Rotated page (/Rotate=270): corner placement TOP-LEFT
    public void Case40_Rotate270_Corner_TopLeft_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case40",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topLeft", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate270 corner TOP-LEFT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r270.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Rotated page (/Rotate=270): corner placement TOP-RIGHT
    public void Case41_Rotate270_Corner_TopRight_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case41",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "topRight", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate270 corner TOP-RIGHT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r270.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Rotated page (/Rotate=270): corner placement BOTTOM-LEFT
    public void Case42_Rotate270_Corner_BottomLeft_Page1()
    {
        RunOnePageFromInput(
        jsonSpec: """
        {
          "overlays": [
            {
              "name": "case42",
              "pages": "1",
              "placement": { "mode": "corner", "corner": "bottomLeft", "offset": [150,72] },
              "primitives": [
                { "type": "rect", "rect": [0,0,260,60], "strokeWidth": 1 },
                { "type": "text", "at": [12,22], "size": 12, "value": "Rotate270 corner BOTTOM-LEFT" }
              ]
            }
          ]
        }
        """,
        inputPdfFileName: "rot-empty-1p_r270.pdf",
        pageNumber1BasedToRender: 1);
    }

    [Fact]
    // Primitive: polyline (stroke) at BOTTOM-RIGHT
    public void Case44_Polyline_Stroke_Page1()
    {
        RunSinglePage("""
          {
            "overlays": [
              {
                "name": "case44",
                "pages": "1",
                "placement": { "mode": "corner", "corner": "bottomRight", "offset": [72,120] },
                "primitives": [
                  { "type": "rect", "rect": [0,0,320,120], "cornerRadius": 10, "fill": "#00000010", "stroke": "#222222ff", "strokeWidth": 1 },
                  {
                    "type": "polyline",
                    "points": [
                      [12, 12],
                      [60, 92],
                      [140, 26],
                      [220, 102],
                      [300, 40]
                    ],
                    "stroke": "#00aa00ff",
                    "strokeWidth": 3
                  },
                  { "type": "text", "at": [12,100], "size": 11, "color": "#222222ff", "value": "Polyline stroke" }
                ]
              }
            ]
          }
          """);
    }

    private static void RunSinglePage(string jsonSpec, [CallerMemberName]string testName = "undefined")
    {
        RunOnePageFromInput(jsonSpec, inputPdfFileName: "empty-10p.pdf", pageNumber1BasedToRender: 1, testName: testName);
    }

    private static void RunOnePageFromInput(string jsonSpec, string inputPdfFileName, int pageNumber1BasedToRender, [CallerMemberName]string testName = "undefined")
    {
        var testData = TestPaths.GetTestDataDir();
        var inputPdf = Path.Combine(testData, "input", inputPdfFileName);

        var workDir = Path.GetTempPath();
        var actualDir = Path.Combine(workDir, "pdftool_test_artifacts", "actual");
        Directory.CreateDirectory(actualDir);

        try
        {
            var expectedDir = Path.Combine(testData, "expected");
            Directory.CreateDirectory(expectedDir);

            // если вызываешь из самого теста:
            var testId = $"{typeof(GoldenMasterRendererTests).FullName}.{testName}";

            var actualPdf = Path.Combine(actualDir, $"{testId}.output.pdf");

            var specPath = WriteTempJson(jsonSpec);

            using (var reader = new PdfReader(inputPdf))
            using (var writer = new PdfWriter(actualPdf))
            using (var pdf = new PdfDocument(reader, writer))
            {
                var spec = JsonSpecParser.Parse(specPath);
                PdfApplyEngine.Apply(pdf, spec);
            }

            var actualPng = Path.Combine(actualDir, $"{testId}.p{pageNumber1BasedToRender}.png");
            PdfRasterizer.RenderPageToPng(actualPdf, pageNumber1Based: pageNumber1BasedToRender, dpi: 144, pngPath: actualPng);

            var expectedPng = Path.Combine(expectedDir, $"{testId}.p{pageNumber1BasedToRender}.png");

            if (!File.Exists(expectedPng))
            {
                if (GoldenTestConfig.UpdateBaselines)
                {
                    File.Copy(actualPng, expectedPng, overwrite: true);
                    return;
                }

                Assert.Fail("Baseline missing. Enable UPDATE_BASELINES.");
            }

            if (GoldenTestConfig.UpdateBaselines)
            {
                File.Copy(actualPng, expectedPng, overwrite: true);
                return;
            }

            ImageComparer.AssertPngEqualWithTolerance(expectedPng, actualPng);
        }
        finally
        {
            Directory.Delete(actualDir, true);    
        }
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
