using System.Text.Json;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Barcodes;
using iText.Barcodes.Qrcode;
using iText.Kernel.Colors;
using iText.PdfCleanup;

namespace PdfTool;

public static class PdfApplyEngine
{
    public static void Apply(PdfDocument pdf, DocumentSpec spec)
    {
        if (pdf is null) throw new ArgumentNullException(nameof(pdf));
        if (spec is null) throw new ArgumentNullException(nameof(spec));

        var totalPages = pdf.GetNumberOfPages();
        var plans = WorkPlanBuilder.Build(spec, totalPages);

        // First pass: redaction for marker-based overlays.
        // IMPORTANT: apply redaction before placing any overlay annotations.
        var anchorPlacements = new List<AnchorPlacement>();
        var hasRedactions = false;

        foreach (var plan in plans)
        {
            if (plan.Placement.Mode != PlacementMode.textAnchor)
                continue;

            foreach (var pageNo in plan.Pages)
            {
                var page = pdf.GetPage(pageNo);

                var matches = TextAnchorSearcher.FindMatches(page, plan.Placement.SearchText!);
                var selected = SelectMatches(matches, plan.Placement.Occurrence);

                if (selected.Count == 0)
                    throw new FormatException($"Overlay '{plan.Name}': marker text '{plan.Placement.SearchText}' was not found on page {pageNo}.");

                foreach (var m in selected)
                {
                    // Redaction (as modification): remove the prepared marker text.
                    var redact = new PdfRedactAnnotation(m.BoundingBox);
                    redact.SetInteriorColor([1f, 1f, 1f]);
                    redact.SetColor(ColorConstants.WHITE);
                    redact.SetFlag(PdfAnnotation.PRINT);
                    page.AddAnnotation(redact);
                    hasRedactions = true;

                    anchorPlacements.Add(new AnchorPlacement(
                        OverlayName: plan.Name,
                        PageNo: pageNo,
                        Placement: plan.Placement,
                        Primitives: plan.Primitives,
                        AnchorTopLeft: m.AnchorTopLeft
                    ));
                }
            }
        }

        if (hasRedactions)
        {
            // Apply redaction annotations.
            PdfCleaner.CleanUpRedactAnnotations(pdf);
        }

        foreach (var plan in plans)
        {
            if (plan.Placement.Mode == PlacementMode.corner)
            {
                if (plan.Placement.Corner is null)
                    throw new FormatException($"Overlay '{plan.Name}': placement.corner is required.");

                foreach (var pageNo in plan.Pages)
                {
                    var page = pdf.GetPage(pageNo);
                    OverlayStampRenderer.RenderOverlayAsStamp(pdf, page, plan.Placement, plan.Primitives, plan.Name);
                }

                continue;
            }

            if (plan.Placement.Mode == PlacementMode.textAnchor)
            {
                // Use the anchor points computed before redaction cleanup.
                foreach (var ap in anchorPlacements)
                {
                    if (!string.Equals(ap.OverlayName, plan.Name, StringComparison.Ordinal))
                        continue;
                    var page = pdf.GetPage(ap.PageNo);
                    OverlayStampRenderer.RenderOverlayAsStampAtTextAnchor(pdf, page, ap.Placement, ap.Primitives, ap.OverlayName, ap.AnchorTopLeft);
                }

                continue;
            }

            throw new NotSupportedException($"placement.mode='{plan.Placement.Mode}' is not supported.");
        }
    }

    private static IReadOnlyList<TextAnchorMatch> SelectMatches(IReadOnlyList<TextAnchorMatch> matches, MarkerOccurrence occurrence)
    {
        if (matches.Count == 0)
            return [];

        return occurrence switch
        {
            MarkerOccurrence.first => [matches[0]],
            MarkerOccurrence.last => [matches[^1]],
            MarkerOccurrence.all => matches,
            _ => throw new ArgumentOutOfRangeException(nameof(occurrence))
        };
    }

    private readonly record struct AnchorPlacement(
        string OverlayName,
        int PageNo,
        PlacementSpec Placement,
        IReadOnlyList<PrimitiveSpec> Primitives,
        PointD AnchorTopLeft
    );
}

internal static class OverlayStampRenderer
{
    public static void RenderOverlayAsStamp(PdfDocument pdf, PdfPage page, PlacementSpec placement, IReadOnlyList<PrimitiveSpec> primitives, string overlayName)
    {
        var bounds = PrimitiveBoundsCalculator.ComputeBoundsMeasured(primitives);
        if (bounds is null)
            return;

        var pageSize = page.GetPageSize();

        var origin = PlacementResolver.ResolveCornerOriginVariantB(
            pageWidth: pageSize.GetWidth(),
            pageHeight: pageSize.GetHeight(),
            corner: placement.Corner!.Value,
            offset: placement.Offset,
            overlayMinX: bounds.Value.MinX,
            overlayMinY: bounds.Value.MinY,
            overlayMaxX: bounds.Value.MaxX,
            overlayMaxY: bounds.Value.MaxY
        );

        var rectOnPage = new Rectangle(
            (float)(origin.X + bounds.Value.MinX),
            (float)(origin.Y + bounds.Value.MinY),
            (float)bounds.Value.Width,
            (float)bounds.Value.Height
        );

        var appearanceBox = new Rectangle(0, 0, rectOnPage.GetWidth(), rectOnPage.GetHeight());
        var xobj = new PdfFormXObject(appearanceBox);

        var shiftOrigin = new PointD(-bounds.Value.MinX, -bounds.Value.MinY);
        PrimitiveRenderer.RenderOnXObject(pdf, xobj, shiftOrigin, primitives);

        var annot = new PdfStampAnnotation(rectOnPage);
        annot.SetContents($"pdftool overlay: {overlayName}");
        annot.SetFlag(PdfAnnotation.PRINT);
        annot.SetNormalAppearance(xobj.GetPdfObject());

        page.AddAnnotation(annot);
    }

    /// <summary>
    /// Places overlay as a stamp annotation anchored to the top-left point of found marker text.
    /// Offset semantics for placement.offset: [dx,dy] where dx is to the right, dy is down (both in pt).
    /// </summary>
    public static void RenderOverlayAsStampAtTextAnchor(
        PdfDocument pdf,
        PdfPage page,
        PlacementSpec placement,
        IReadOnlyList<PrimitiveSpec> primitives,
        string overlayName,
        PointD anchorTopLeft)
    {
        var bounds = PrimitiveBoundsCalculator.ComputeBoundsMeasured(primitives);
        if (bounds is null)
            return;

        var dx = 0d;
        var dy = 0d;
        if (placement.Offset is not null)
        {
            if (placement.Offset.Length != 2)
                throw new ArgumentException("offset must be [dx,dy]", nameof(placement));
            dx = placement.Offset[0];
            dy = placement.Offset[1];
        }

        var targetX = anchorTopLeft.X + dx;
        var targetY = anchorTopLeft.Y - dy; // down

        // Align overlay's top-left (in overlay-local bounds) to the target point.
        // origin + (overlayMinX, overlayMaxY) = (targetX, targetY)
        var origin = new PointD(
            targetX - bounds.Value.MinX,
            targetY - bounds.Value.MaxY
        );

        var rectOnPage = new Rectangle(
            (float)(origin.X + bounds.Value.MinX),
            (float)(origin.Y + bounds.Value.MinY),
            (float)bounds.Value.Width,
            (float)bounds.Value.Height
        );

        var appearanceBox = new Rectangle(0, 0, rectOnPage.GetWidth(), rectOnPage.GetHeight());
        var xobj = new PdfFormXObject(appearanceBox);

        var shiftOrigin = new PointD(-bounds.Value.MinX, -bounds.Value.MinY);
        PrimitiveRenderer.RenderOnXObject(pdf, xobj, shiftOrigin, primitives);

        var annot = new PdfStampAnnotation(rectOnPage);
        annot.SetContents($"pdftool overlay: {overlayName}");
        annot.SetFlag(PdfAnnotation.PRINT);
        annot.SetNormalAppearance(xobj.GetPdfObject());

        page.AddAnnotation(annot);
    }
}

internal readonly record struct BoundsD(double MinX, double MinY, double MaxX, double MaxY)
{
    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;
}

internal static class PrimitiveBoundsCalculator
{
    public static BoundsD? ComputeBoundsMeasured(IReadOnlyList<PrimitiveSpec> primitives)
    {
        if (primitives.Count == 0) 
            return null;

        var hasAny = false;
        double minX = 0, minY = 0, maxX = 0, maxY = 0;

        void Include(double x0, double y0, double x1, double y1)
        {
            var loX = Math.Min(x0, x1);
            var hiX = Math.Max(x0, x1);
            var loY = Math.Min(y0, y1);
            var hiY = Math.Max(y0, y1);

            if (!hasAny)
            {
                minX = loX; minY = loY; maxX = hiX; maxY = hiY;
                hasAny = true;
            }
            else
            {
                minX = Math.Min(minX, loX);
                minY = Math.Min(minY, loY);
                maxX = Math.Max(maxX, hiX);
                maxY = Math.Max(maxY, hiY);
            }
        }

        foreach (var prim in primitives)
        {
            switch (prim)
            {
                case RectPrimitiveSpec r:
                    Include(r.Rect[0], r.Rect[1], r.Rect[0] + r.Rect[2], r.Rect[1] + r.Rect[3]);
                    break;

                case LinePrimitiveSpec l:
                    {
                        var sw = l.StrokeWidth ?? 1;
                        Include(l.From[0] - sw / 2, l.From[1] - sw / 2, l.To[0] + sw / 2, l.To[1] + sw / 2);
                        break;
                    }

                case BarcodePrimitiveSpec bc:
                    // Bounds for barcodes are defined by their rect.
                    Include(bc.Rect[0], bc.Rect[1], bc.Rect[0] + bc.Rect[2], bc.Rect[1] + bc.Rect[3]);
                    break;

                case ImagePrimitiveSpec img:
                    {
                        // Image primitive currently supports only rect: [x,y,w,h]
                        Include(img.Rect[0], img.Rect[1], img.Rect[0] + img.Rect[2], img.Rect[1] + img.Rect[3]);
                        break;
                    }

                case TextPrimitiveSpec t:
                    {
                        if (t.Rect is not null)
                        {
                            Include(t.Rect[0], t.Rect[1], t.Rect[0] + t.Rect[2], t.Rect[1] + t.Rect[3]);
                        }
                        else if (t.At is not null)
                        {
                            var fontSize = (float)(t.Size ?? 12);
                            var w = MeasurePointTextWidth(t.Value, fontSize);
                            var h = Math.Max(1f, fontSize * 1.2f);

                            Include(t.At[0], t.At[1], t.At[0] + w, t.At[1] + h);
                        }
                        break;
                    }
            }
        }

        if (!hasAny) return null;

        if (maxX - minX <= 0) maxX = minX + 1;
        if (maxY - minY <= 0) maxY = minY + 1;

        return new BoundsD(minX, minY, maxX, maxY);
    }

    internal static float MeasurePointTextWidth(string text, float fontSize)
    {
        // Using a standard built-in font for measurement.
        // This matches the default font iText uses when none is explicitly set.
        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // PdfFont.GetWidth returns width in glyph units; overload with fontSize yields width in points.
        var w = font.GetWidth(text, fontSize);

        // Safety: avoid zero width (empty text) producing degenerate bounds.
        return Math.Max(1f, w);
    }

    internal static float MeasureIntrinsicTextWidth(string text, float fontSize)
    {
        // Measures the widest line (split by \n) using a standard built-in font.
        // Used to implement text.rect.halign as a BLOCK alignment (not text alignment inside the block).
        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        if (string.IsNullOrEmpty(text))
            return 1f;

        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var max = 1f;

        foreach (var line in normalized.Split('\n'))
        {
            var w = font.GetWidth(line, fontSize);
            if (w > max) max = w;
        }

        return Math.Max(1f, max);
    }

}

public static class PrimitiveRenderer
{
    public static void RenderOnXObject(PdfDocument pdf, PdfFormXObject xobj, PointD origin, IReadOnlyList<PrimitiveSpec> primitives)
    {
        var pdfCanvas = new PdfCanvas(xobj, pdf);
        var canvas = new Canvas(pdfCanvas, new Rectangle(0, 0, xobj.GetWidth(), xobj.GetHeight()));

        foreach (var prim in primitives)
        {
            switch (prim)
            {
                case RectPrimitiveSpec r:
                    DrawRect(pdfCanvas, origin, r);
                    break;

                case LinePrimitiveSpec l:
                    DrawLine(pdfCanvas, origin, l);
                    break;

                case BarcodePrimitiveSpec bc:
                    DrawBarcode(pdfCanvas, pdf, origin, bc);
                    break;

                case ImagePrimitiveSpec img:
                    DrawImage(pdfCanvas, origin, img);
                    break;

                case TextPrimitiveSpec t:
                    DrawText(canvas, origin, t);
                    break;

                default:
                    throw new NotSupportedException($"Primitive '{prim.GetType().Name}' is not implemented in this renderer iteration.");
            }
        }

        canvas.Close();
    }

    private static void DrawRect(PdfCanvas c, PointD o, RectPrimitiveSpec r)
    {
        var x = (float)(o.X + r.Rect[0]);
        var y = (float)(o.Y + r.Rect[1]);
        var w = (float)r.Rect[2];
        var h = (float)r.Rect[3];

        var cr = (float)(r.CornerRadius ?? 0);
        if (cr > 0)
            c.RoundRectangle(x, y, w, h, cr);
        else
            c.Rectangle(x, y, w, h);

        if (r.StrokeWidth is not null && r.StrokeWidth.Value > 0)
            c.SetLineWidth((float)r.StrokeWidth.Value);

        var hasFill = !string.IsNullOrWhiteSpace(r.Fill);
        var hasStroke = !string.IsNullOrWhiteSpace(r.Stroke) || (r.StrokeWidth is not null && r.StrokeWidth.Value > 0);

        if (hasFill && hasStroke) c.FillStroke();
        else if (hasFill) c.Fill();
        else c.Stroke();
    }

    private static void DrawLine(PdfCanvas c, PointD o, LinePrimitiveSpec l)
    {
        var x1 = (float)(o.X + l.From[0]);
        var y1 = (float)(o.Y + l.From[1]);
        var x2 = (float)(o.X + l.To[0]);
        var y2 = (float)(o.Y + l.To[1]);

        if (l.StrokeWidth is not null && l.StrokeWidth.Value > 0)
            c.SetLineWidth((float)l.StrokeWidth.Value);

        c.MoveTo(x1, y1);
        c.LineTo(x2, y2);
        c.Stroke();
    }

    private static void DrawBarcode(PdfCanvas c, PdfDocument pdf, PointD o, BarcodePrimitiveSpec bc)
    {
        var x = (float)(o.X + bc.Rect[0]);
        var y = (float)(o.Y + bc.Rect[1]);
        var w = (float)bc.Rect[2];
        var h = (float)bc.Rect[3];

        var target = new Rectangle(x, y, w, h);

        if (bc.Kind == BarcodeKind.qr)
        {
            IDictionary<EncodeHintType, object>? hints = null;

            if (bc.Options is not null && bc.Options.Value.ValueKind == JsonValueKind.Object &&
                bc.Options.Value.TryGetProperty("ecLevel", out var ec) && ec.ValueKind == JsonValueKind.String)
            {
                var s = ec.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    hints = new Dictionary<EncodeHintType, object>();
                    hints[EncodeHintType.ERROR_CORRECTION] = s switch
                    {
                        "L" => ErrorCorrectionLevel.L,
                        "M" => ErrorCorrectionLevel.M,
                        "Q" => ErrorCorrectionLevel.Q,
                        "H" => ErrorCorrectionLevel.H,
                        _ => ErrorCorrectionLevel.L
                    };
                }
            }

            var qr = hints is null ? new BarcodeQRCode(bc.Value) : new BarcodeQRCode(bc.Value, hints);
            var xobj = qr.CreateFormXObject(ColorConstants.BLACK, pdf);

            // Fit into target rect (keeps proportions)
            c.AddXObjectFittedIntoRectangle(xobj, target);
            return;
        }

        if (bc.Kind == BarcodeKind.code128)
        {
            var b128 = new Barcode128(pdf);
            b128.SetCodeType(Barcode128.CODE128);
            b128.SetCode(bc.Value);

            var showText = true;
            if (bc.Options is not null && bc.Options.Value.ValueKind == JsonValueKind.Object &&
                bc.Options.Value.TryGetProperty("showText", out var st) &&
                (st.ValueKind == JsonValueKind.True || st.ValueKind == JsonValueKind.False))
            {
                showText = st.GetBoolean();
            }

            // Ensure a font is present for the human-readable line
            b128.SetFont(!showText ? null : PdfFontFactory.CreateFont(StandardFonts.HELVETICA));

            var xobj = b128.CreateFormXObject(ColorConstants.BLACK, ColorConstants.BLACK, pdf);
            c.AddXObjectFittedIntoRectangle(xobj, target);
            return;
        }

        throw new NotSupportedException($"Unsupported barcode kind: {bc.Kind}");
    }

    private static void DrawImage(PdfCanvas c, PointD o, ImagePrimitiveSpec img)
    {
        if (string.IsNullOrWhiteSpace(img.Base64))
            throw new FormatException("Image primitive: base64 is required.");

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(img.Base64);
        }
        catch (Exception ex)
        {
            throw new FormatException("Image primitive: data.base64 is not valid base64.", ex);
        }

        ImageData data;
        try
        {
            data = ImageDataFactory.Create(bytes);
        }
        catch (Exception ex)
        {
            throw new FormatException("Image primitive: failed to decode image bytes.", ex);
        }

        var x = (float)(o.X + img.Rect[0]);
        var y = (float)(o.Y + img.Rect[1]);
        var w = (float)img.Rect[2];
        var h = (float)img.Rect[3];

        var r = new Rectangle(x, y, w, h);
        c.AddImageFittedIntoRectangle(data, r, true);
    }

    private static void DrawText(Canvas canvas, PointD o, TextPrimitiveSpec t)
    {
        if (t.At is not null)
        {
            var x = (float)(o.X + t.At[0]);
            var y = (float)(o.Y + t.At[1]);

            var p = new Paragraph(t.Value);
            p.SetMargin(0);
            p.SetPadding(0);

            var fontSize = (float)(t.Size ?? 12);
            p.SetFontSize(fontSize);

            // IMPORTANT: do not use large constant width here — it breaks bounds/anchoring for topRight.
            var w = PrimitiveBoundsCalculator.MeasurePointTextWidth(t.Value, fontSize);
            p.SetFixedPosition(x, y, w);

            canvas.Add(p);
            return;
        }

        if (t.Rect is not null)
        {
            var x = (float)(o.X + t.Rect[0]);
            var y = (float)(o.Y + t.Rect[1]);
            var w = (float)t.Rect[2];
            var h = (float)t.Rect[3];

            var p = new Paragraph(t.Value);

            if (t.Size is not null)
                p.SetFontSize((float)t.Size.Value);

            if (t.Align is not null)
            {
                var align = t.Align.Value switch
                {
                    TextAlign.left => TextAlignment.LEFT,
                    TextAlign.center => TextAlignment.CENTER,
                    TextAlign.right => TextAlignment.RIGHT,
                    TextAlign.justify => TextAlignment.JUSTIFIED,
                    _ => TextAlignment.LEFT
                };
                p.SetTextAlignment(align);
            }

            // Measure height with layout engine. For block width:
            // - wrap=true  => block spans the whole rect width (halign becomes a no-op, as expected)
            // - wrap=false => block width is the intrinsic text width (clamped to rect width), so halign can shift it.
            var wrap = t.Wrap ?? false;

            var blockWidth = w;
            if (!wrap)
            {
                var fs = (float)(t.Size ?? 12);
                blockWidth = Math.Min(w, PrimitiveBoundsCalculator.MeasureIntrinsicTextWidth(t.Value, fs));
            }

            var metrics = MeasureParagraph(canvas, p, blockWidth);

            var valign = t.VAlign ?? VerticalAlign.top;
            var yStart = valign switch
            {
                VerticalAlign.bottom => y,
                VerticalAlign.middle => y + (h - metrics.Height) / 2f,
                _ => y + (h - metrics.Height)
            };

            var halign = t.HAlign ?? HorizontalAlign.left;
            var dx = Math.Max(0, w - blockWidth);
            var xStart = halign switch
            {
                HorizontalAlign.center => x + dx / 2f,
                HorizontalAlign.right => x + dx,
                _ => x
            };

            var pdfCanvas = canvas.GetPdfCanvas();
            pdfCanvas.SaveState();
            pdfCanvas.Rectangle(x, y, w, h);
            pdfCanvas.Clip();
            pdfCanvas.EndPath();

            var layoutArea = new Rectangle(xStart, yStart, Math.Max(blockWidth, 1), Math.Max(metrics.Height, 1));
            using (var blockCanvas = new Canvas(pdfCanvas, layoutArea))
            {
                blockCanvas.Add(p);
            }

            pdfCanvas.RestoreState();
            return;
        }

        throw new InvalidOperationException("Text primitive must have either 'at' or 'rect'.");
    }

    private readonly record struct ParagraphMetrics(float Height);

    private static ParagraphMetrics MeasureParagraph(Canvas canvas, Paragraph p, float width)
    {
        const float hugeHeight = 10_000f;
        var measureRect = new Rectangle(0, 0, width, hugeHeight);

        using var measureCanvas = new Canvas(canvas.GetPdfCanvas(), measureRect);

        var renderer = p.CreateRendererSubTree();
        renderer.SetParent(measureCanvas.GetRenderer());

        var result = renderer.Layout(new LayoutContext(new LayoutArea(1, measureRect)));

        if (result.GetStatus() != LayoutResult.FULL)
            return new ParagraphMetrics(hugeHeight);

        var bbox = result.GetOccupiedArea().GetBBox();
        var hOcc = Math.Max(1, bbox.GetHeight());
        return new ParagraphMetrics(hOcc);
    }
}
