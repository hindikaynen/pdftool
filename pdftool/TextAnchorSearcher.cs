using System.Text;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfTool;

internal sealed record TextAnchorMatch(
    Rectangle BoundingBox,
    PointD AnchorTopLeft
);

internal static class TextAnchorSearcher
{
    public static IReadOnlyList<TextAnchorMatch> FindMatches(PdfPage page, string searchText)
    {
        if (page is null) throw new ArgumentNullException(nameof(page));
        if (string.IsNullOrEmpty(searchText)) throw new ArgumentException("searchText must be non-empty", nameof(searchText));

        var listener = new CharLocationListener();
        var processor = new PdfCanvasProcessor(listener);
        processor.ProcessPageContent(page);

        if (listener.Chars.Count == 0)
            return Array.Empty<TextAnchorMatch>();

        var text = listener.GetText();
        var matches = new List<TextAnchorMatch>();

        var startIndex = 0;
        while (startIndex < text.Length)
        {
            var idx = text.IndexOf(searchText, startIndex, StringComparison.Ordinal);
            if (idx < 0)
                break;

            var bbox = Union(listener.Chars, idx, searchText.Length);
            var anchor = new PointD(bbox.GetX(), bbox.GetY() + bbox.GetHeight());
            matches.Add(new TextAnchorMatch(bbox, anchor));

            // Allow overlapping matches by advancing by 1.
            startIndex = idx + 1;
        }

        return matches;
    }

    private static Rectangle Union(IReadOnlyList<CharInfo> chars, int start, int length)
    {
        var end = start + length;
        if (start < 0 || end > chars.Count)
            throw new ArgumentOutOfRangeException(nameof(start));

        var first = chars[start].Rect;
        float minX = first.GetX();
        float minY = first.GetY();
        float maxX = first.GetX() + first.GetWidth();
        float maxY = first.GetY() + first.GetHeight();

        for (var i = start + 1; i < end; i++)
        {
            var r = chars[i].Rect;
            var x0 = r.GetX();
            var y0 = r.GetY();
            var x1 = x0 + r.GetWidth();
            var y1 = y0 + r.GetHeight();

            if (x0 < minX) minX = x0;
            if (y0 < minY) minY = y0;
            if (x1 > maxX) maxX = x1;
            if (y1 > maxY) maxY = y1;
        }

        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }
}

internal readonly record struct CharInfo(char Ch, Rectangle Rect);

internal sealed class CharLocationListener : IEventListener
{
    public List<CharInfo> Chars { get; } = new();

    public void EventOccurred(IEventData data, EventType type)
    {
        if (type != EventType.RENDER_TEXT)
            return;

        var renderInfo = (TextRenderInfo)data;
        foreach (var cri in renderInfo.GetCharacterRenderInfos())
        {
            var t = cri.GetText();
            if (string.IsNullOrEmpty(t))
                continue;

            var ascent = cri.GetAscentLine().GetBoundingRectangle();
            var descent = cri.GetDescentLine().GetBoundingRectangle();

            var minX = Math.Min(ascent.GetX(), descent.GetX());
            var minY = Math.Min(ascent.GetY(), descent.GetY());
            var maxX = Math.Max(ascent.GetX() + ascent.GetWidth(), descent.GetX() + descent.GetWidth());
            var maxY = Math.Max(ascent.GetY() + ascent.GetHeight(), descent.GetY() + descent.GetHeight());

            var rect = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            Chars.Add(new CharInfo(t[0], rect));
        }
    }

    public ICollection<EventType> GetSupportedEvents() => new HashSet<EventType> { EventType.RENDER_TEXT };

    public string GetText()
    {
        if (Chars.Count == 0) return string.Empty;
        var sb = new StringBuilder(Chars.Count);
        foreach (var c in Chars)
            sb.Append(c.Ch);
        return sb.ToString();
    }
}
