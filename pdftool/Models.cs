using System.Text.Json;
using System.Text.Json.Serialization;
using PdfTool;

namespace PdfTool;

public sealed class DocumentSpec
{
    [JsonPropertyName("overlays")]
    public List<OverlaySpec> Overlays { get; init; } = new();
}

public sealed class OverlaySpec
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    // Pages range string, e.g. "1-3,7-last", "all"
    [JsonPropertyName("pages")]
    public required string Pages { get; init; }

    [JsonPropertyName("placement")]
    public required PlacementSpec Placement { get; init; }

    [JsonPropertyName("primitives")]
    public List<PrimitiveSpec> Primitives { get; init; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlacementMode { corner, textAnchor }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PageCorner { topLeft, topRight, bottomLeft, bottomRight }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MarkerOccurrence { first, last, all }

public sealed class PlacementSpec
{
    [JsonPropertyName("mode")]
    public required PlacementMode Mode { get; init; }

    // Corner mode
    [JsonPropertyName("corner")]
    public PageCorner? Corner { get; init; }

    // Marker mode
    [JsonPropertyName("text")]
    public string? SearchText { get; init; }

    [JsonPropertyName("occurrence")]
    public MarkerOccurrence Occurrence { get; init; } = MarkerOccurrence.first;

    // Common offset [dx, dy] in pt
    [JsonPropertyName("offset")]
    public double[]? Offset { get; init; }
}

// -------- Primitives (polymorphic via "type") --------

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextPrimitiveSpec), "text")]
[JsonDerivedType(typeof(RectPrimitiveSpec), "rect")]
[JsonDerivedType(typeof(LinePrimitiveSpec), "line")]
[JsonDerivedType(typeof(ImagePrimitiveSpec), "image")]
[JsonDerivedType(typeof(BarcodePrimitiveSpec), "barcode")]
public abstract class PrimitiveSpec
{
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TextAlign { left, center, right, justify }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HorizontalAlign { left, center, right }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerticalAlign { top, middle, bottom }

public sealed class TextPrimitiveSpec : PrimitiveSpec
{
    // Either:
    //   at: [x,y]
    // Or:
    //   rect: [x,y,w,h]
    [JsonPropertyName("at")]
    public double[]? At { get; init; }

    [JsonPropertyName("rect")]
    public double[]? Rect { get; init; }

    [JsonPropertyName("wrap")]
    public bool? Wrap { get; init; }

    [JsonPropertyName("align")]
    public TextAlign? Align { get; init; }

    [JsonPropertyName("valign")]
    public VerticalAlign? VAlign { get; init; }

    [JsonPropertyName("halign")]
    public HorizontalAlign? HAlign { get; init; }

    [JsonPropertyName("font")]
    public string? Font { get; init; }

    [JsonPropertyName("size")]
    public double? Size { get; init; }

    [JsonPropertyName("color")]
    public string? Color { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

public sealed class RectPrimitiveSpec : PrimitiveSpec
{
    // rect: [x,y,w,h]
    [JsonPropertyName("rect")]
    public required double[] Rect { get; init; }

    [JsonPropertyName("cornerRadius")]
    public double? CornerRadius { get; init; }

    [JsonPropertyName("fill")]
    public string? Fill { get; init; }

    [JsonPropertyName("stroke")]
    public string? Stroke { get; init; }

    [JsonPropertyName("strokeWidth")]
    public double? StrokeWidth { get; init; }
}

public sealed class LinePrimitiveSpec : PrimitiveSpec
{
    [JsonPropertyName("from")]
    public required double[] From { get; init; }

    [JsonPropertyName("to")]
    public required double[] To { get; init; }

    [JsonPropertyName("stroke")]
    public string? Stroke { get; init; }

    [JsonPropertyName("strokeWidth")]
    public double? StrokeWidth { get; init; }
}

class ImagePrimitiveSpec : PrimitiveSpec
{
    public double[] Rect { get; init; }
    public string Base64 { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BarcodeKind { qr, code128 }

public sealed class BarcodePrimitiveSpec : PrimitiveSpec
{
    [JsonPropertyName("kind")]
    public required BarcodeKind Kind { get; init; }

    // rect: [x,y,w,h]
    [JsonPropertyName("rect")]
    public required double[] Rect { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }

    // Arbitrary per-kind options
    [JsonPropertyName("options")]
    public JsonElement? Options { get; init; }
}
