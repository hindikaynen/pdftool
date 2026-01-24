using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace PdfTool;

public static class PdfApplyEngine
{
    /// <summary>
    /// Applies overlays to an already opened PdfDocument (reader+writer).
    /// Current iteration:
    /// - placement: corner only
    /// - primitives: rect, line, text(at) only
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

                PrimitiveRenderer.RenderOnPage(pdf, page, origin, plan.Primitives);
            }
        }
    }
}

public static class PrimitiveRenderer
{
    public static void RenderOnPage(PdfDocument pdf, PdfPage page, PointD origin, IReadOnlyList<PrimitiveSpec> primitives)
    {
        var pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
        var canvas = new Canvas(pdfCanvas, page.GetPageSize());

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

        // NOTE: in this iteration we don't parse colors; draw a simple rectangle using default graphics state.
        c.Rectangle(x, y, w, h);

        if (r.StrokeWidth is not null && r.StrokeWidth.Value > 0)
            c.SetLineWidth((float)r.StrokeWidth.Value);

        // Fill/stroke selection
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
        if (t.At is null)
            throw new NotSupportedException("Only text.at is implemented in this iteration (no text.rect yet).");

        var x = (float)(o.X + t.At[0]);
        var y = (float)(o.Y + t.At[1]);

        var p = new Paragraph(t.Value);

        if (t.Size is not null)
            p.SetFontSize((float)t.Size.Value);

        // Use absolute position: fixed position uses bottom-left of the text box
        p.SetFixedPosition(x, y, 1000);

        canvas.Add(p);
    }
}
