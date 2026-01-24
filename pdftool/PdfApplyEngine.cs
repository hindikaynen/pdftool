using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace PdfTool;

public static class PdfApplyEngine
{
    /// <summary>
    /// Applies overlays to an already opened PdfDocument (reader+writer).
    ///
    /// Policy (fixed):
    /// - All overlays are rendered as Stamp annotations (Rubber Stamp) with appearance streams.
    /// - Redaction (textMarker) will be implemented later as a separate content-modification step.
    ///
    /// Current iteration supports:
    /// - placement: corner only
    /// - primitives: rect(+cornerRadius), line, text(at), text(rect+wrap)
    /// </summary>
    public static void Apply(PdfDocument pdf, DocumentSpec spec)
    {
        if (pdf is null) throw new ArgumentNullException(nameof(pdf));
        if (spec is null) throw new ArgumentNullException(nameof(spec));

        var totalPages = pdf.GetNumberOfPages();
        var plans = WorkPlanBuilder.Build(spec, totalPages);

        foreach (var plan in plans)
        {
            if (plan.Placement.Mode != PlacementMode.corner)
                throw new NotSupportedException("Only placement.mode='corner' is implemented in this iteration.");

            if (plan.Placement.Corner is null)
                throw new FormatException($"Overlay '{plan.Name}': placement.corner is required.");

            foreach (var pageNo in plan.Pages)
            {
                var page = pdf.GetPage(pageNo);
                var pageSize = page.GetPageSize();

                var origin = PlacementResolver.ResolveCornerOrigin(
                    pageWidth: pageSize.GetWidth(),
                    pageHeight: pageSize.GetHeight(),
                    corner: plan.Placement.Corner.Value,
                    offset: plan.Placement.Offset
                );

                OverlayStampRenderer.RenderOverlayAsStamp(pdf, page, origin, plan.Primitives, plan.Name);
            }
        }
    }
}

internal static class OverlayStampRenderer
{
    public static void RenderOverlayAsStamp(PdfDocument pdf, PdfPage page, PointD origin, IReadOnlyList<PrimitiveSpec> primitives, string overlayName)
    {
        var bounds = PrimitiveBoundsCalculator.ComputeBounds(primitives);

        // If nothing has measurable bounds, skip.
        if (bounds is null)
            return;

        // Place bounds on the page
        var rectOnPage = new Rectangle(
            (float)(origin.X + bounds.Value.MinX),
            (float)(origin.Y + bounds.Value.MinY),
            (float)bounds.Value.Width,
            (float)bounds.Value.Height
        );

        // Create appearance XObject with bbox starting at (0,0)
        var appearanceBox = new Rectangle(0, 0, rectOnPage.GetWidth(), rectOnPage.GetHeight());
        var xobj = new PdfFormXObject(appearanceBox);

        // Draw into appearance with a shifted origin so that primitive local coordinates map into [0..w, 0..h]
        var shiftOrigin = new PointD(-bounds.Value.MinX, -bounds.Value.MinY);
        PrimitiveRenderer.RenderOnXObject(pdf, xobj, shiftOrigin, primitives);

        // Create stamp annotation and assign appearance
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
    public static BoundsD? ComputeBounds(IReadOnlyList<PrimitiveSpec> primitives)
    {
        if (primitives is null || primitives.Count == 0) return null;

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
                {
                    Include(r.Rect[0], r.Rect[1], r.Rect[0] + r.Rect[2], r.Rect[1] + r.Rect[3]);
                    break;
                }
                case LinePrimitiveSpec l:
                {
                    var sw = l.StrokeWidth ?? 1;
                    Include(l.From[0] - sw / 2, l.From[1] - sw / 2, l.To[0] + sw / 2, l.To[1] + sw / 2);
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
                        // Conservative bounds for point text: width is the same constant used in SetFixedPosition.
                        var fontSize = t.Size ?? 12;
                        Include(t.At[0], t.At[1], t.At[0] + 1000, t.At[1] + fontSize * 1.2);
                    }
                    break;
                }
            }
        }

        if (!hasAny) return null;

        // Ensure non-zero bbox
        var width = maxX - minX;
        var height = maxY - minY;
        if (width <= 0) { maxX = minX + 1; }
        if (height <= 0) { maxY = minY + 1; }

        return new BoundsD(minX, minY, maxX, maxY);
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

    private static void DrawText(Canvas canvas, PointD o, TextPrimitiveSpec t)
    {
        if (t.At is not null)
        {
            var x = (float)(o.X + t.At[0]);
            var y = (float)(o.Y + t.At[1]);

            var p = new Paragraph(t.Value);

            if (t.Size is not null)
                p.SetFontSize((float)t.Size.Value);

            p.SetFixedPosition(x, y, 1000);

            canvas.Add(p);
            return;
        }

        if (t.Rect is not null)
        {
            var x = (float)(o.X + t.Rect[0]);
            var y = (float)(o.Y + t.Rect[1]);
            var w = (float)t.Rect[2];
            var h = (float)t.Rect[3];

            var area = new Rectangle(x, y, w, h);
            using var blockCanvas = new Canvas(canvas.GetPdfCanvas(), area);

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

            blockCanvas.Add(p);
            return;
        }

        throw new InvalidOperationException("Text primitive must have either 'at' or 'rect'.");
    }
}
