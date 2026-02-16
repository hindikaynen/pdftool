using System.Text.Json;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// Page range string, e.g. "1-3,7-last, all"
    /// </summary>
    [JsonPropertyName("pages")]
    public required string Pages { get; init; }

    [JsonPropertyName("placement")]
    public required PlacementSpec Placement { get; init; }

    [JsonPropertyName("primitives")]
    public List<PrimitiveSpec> Primitives { get; init; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter<PlacementMode>))]
public enum PlacementMode
{
    corner,
    textAnchor,
}

[JsonConverter(typeof(JsonStringEnumConverter<PageCorner>))]
public enum PageCorner
{
    topLeft,
    topRight,
    bottomLeft,
    bottomRight
}

[JsonConverter(typeof(JsonStringEnumConverter<MarkerOccurrence>))]
public enum MarkerOccurrence
{
    first,
    last,
    all,
}

public sealed class PlacementSpec
{
    [JsonPropertyName("mode")]
    public required PlacementMode Mode { get; init; }

    /// <summary>
    /// Optional per-overlay-instance placement overrides.
    /// Key format: "{baseName}#p{pageNo}#i{index}".
    /// Value: absolute TOP-LEFT [x,y] in user-facing VIEW coordinates (origin at page top-left, Y-down).
    /// </summary>
    [JsonPropertyName("overrides")]
    public Dictionary<string, double[]>? Overrides { get; init; }

    /// <summary>
    /// Corner mode
    /// </summary>
    [JsonPropertyName("corner")]
    public PageCorner? Corner { get; init; }

    /// <summary>
    /// Marker mode
    /// </summary>
    [JsonPropertyName("text")]
    public string? SearchText { get; init; }

    [JsonPropertyName("occurrence")]
    public MarkerOccurrence Occurrence { get; init; } = MarkerOccurrence.first;

    /// <summary>
    /// Common offset [dx, dy] in pt
    /// </summary>
    [JsonPropertyName("offset")]
    public double[]? Offset { get; init; }
}

/// -------- Primitives: polymorphic via type --------
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextPrimitiveSpec), "text")]
[JsonDerivedType(typeof(RectPrimitiveSpec), "rect")]
[JsonDerivedType(typeof(EllipsePrimitiveSpec), "ellipse")]
[JsonDerivedType(typeof(LinePrimitiveSpec), "line")]
[JsonDerivedType(typeof(PolylinePrimitiveSpec), "polyline")]
[JsonDerivedType(typeof(ImagePrimitiveSpec), "image")]
[JsonDerivedType(typeof(BarcodePrimitiveSpec), "barcode")]
public abstract class PrimitiveSpec
{
}

[JsonConverter(typeof(JsonStringEnumConverter<TextAlign>))]
public enum TextAlign
{
    left,
    center,
    right
}

[JsonConverter(typeof(JsonStringEnumConverter<HorizontalAlign>))]
public enum HorizontalAlign
{
    left,
    center,
    right,
}

[JsonConverter(typeof(JsonStringEnumConverter<VerticalAlign>))]
public enum VerticalAlign
{
    top,
    middle,
    bottom,
}

public sealed class TextPrimitiveSpec : PrimitiveSpec
{
    /// <summary>
    /// Either [at: x,y] Or [rect: x,y,w,h]
    /// </summary>
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
    /// <summary>
    /// rect [x,y,w,h]
    /// </summary>
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

public sealed class EllipsePrimitiveSpec : PrimitiveSpec
{
    /// <summary>
    /// rect [x,y,w,h] - bounding box of the ellipse
    /// </summary>
    [JsonPropertyName("rect")]
    public required double[] Rect { get; init; }

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

public sealed class PolylinePrimitiveSpec : PrimitiveSpec
{
    /// <summary>
    /// points [[x,y], [x,y], ...]
    /// </summary>
    [JsonPropertyName("points")]
    public required double[][] Points { get; init; }

    [JsonPropertyName("stroke")]
    public string? Stroke { get; init; }

    [JsonPropertyName("strokeWidth")]
    public double? StrokeWidth { get; init; }
}

public sealed class ImagePrimitiveSpec : PrimitiveSpec
{
    [JsonPropertyName("rect")]
    public required double[] Rect { get; init; }

    [JsonPropertyName("base64")]
    public required string Base64 { get; init; }

    [JsonPropertyName("fill")]
    public string? Fill { get; init; }

    [JsonPropertyName("stroke")]
    public string? Stroke { get; init; }

    [JsonPropertyName("strokeWidth")]
    public double? StrokeWidth { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<BarcodeKind>))]
public enum BarcodeKind
{
    qr,
    code128,
}

public sealed class BarcodePrimitiveSpec : PrimitiveSpec
{
    [JsonPropertyName("kind")]
    public required BarcodeKind Kind { get; init; }

    /// <summary>
    /// rect [x,y,w,h]
    /// </summary>
    [JsonPropertyName("rect")]
    public required double[] Rect { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }

    /// <summary>
    /// Arbitrary per-kind options
    /// </summary>
    [JsonPropertyName("options")]
    public JsonElement? Options { get; init; }

    /// <summary>
    /// Optional coloring and border/background
    /// </summary>
    [JsonPropertyName("color")]
    public string? Color { get; init; }

    [JsonPropertyName("fill")]
    public string? Fill { get; init; }

    [JsonPropertyName("stroke")]
    public string? Stroke { get; init; }

    [JsonPropertyName("strokeWidth")]
    public double? StrokeWidth { get; init; }
}
