using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfTool;

internal static class TextAnchorSearcher
{
    public static List<Rectangle> FindMatchingTextBoxes(PdfPage page, string searchText)
    {
        if (page is null) throw new ArgumentNullException(nameof(page));
        if (string.IsNullOrEmpty(searchText)) return new List<Rectangle>();

        var listener = new TextBoxCollectingListener(searchText);
        var processor = new PdfCanvasProcessor(listener);
        processor.ProcessPageContent(page);

        return listener.Boxes;
    }

    public static List<Rectangle> SelectOccurrences(IReadOnlyList<Rectangle> matches, MarkerOccurrence occurrence)
    {
        if (matches.Count == 0) 
            return new List<Rectangle>();

        return occurrence switch
        {
            MarkerOccurrence.first => [matches[0]],
            MarkerOccurrence.last => [matches[^1]],
            MarkerOccurrence.all => [..matches],
            _ => [matches[0]]
        };
    }

    private sealed class TextBoxCollectingListener(string needle) : IEventListener
    {
        public List<Rectangle> Boxes { get; } = new();

        // Sliding window of the last N characters (N = needle.Length) with their bboxes.
        // iText may emit text in very small chunks (down to 1 glyph), so we match
        // across RENDER_TEXT events reliably by working per-character.
        private readonly Queue<(string ch, Rectangle rect)> _window = new();
        private string _windowText = string.Empty;

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT) return;

            var tri = (TextRenderInfo)data;

            foreach (var cri in tri.GetCharacterRenderInfos())
            {
                var ch = cri.GetText();
                if (string.IsNullOrEmpty(ch)) continue;

                var rect = GetCharRect(cri);

                _window.Enqueue((ch, rect));
                _windowText += ch;

                // Keep window length capped to needle.Length (handle multi-char glyphs too)
                while (_windowText.Length > needle.Length && _window.Count > 0)
                {
                    var removed = _window.Dequeue();
                    _windowText = _windowText.Substring(removed.ch.Length);
                }

                if (_windowText == needle && _window.Count > 0)
                {
                    Boxes.Add(UnionRects(_window));

                    // Move forward by one character to allow subsequent matches.
                    var removed = _window.Dequeue();
                    _windowText = _windowText.Substring(removed.ch.Length);
                }
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new HashSet<EventType> { EventType.RENDER_TEXT };
        }

        private static Rectangle GetCharRect(TextRenderInfo charRenderInfo)
        {
            var asc = charRenderInfo.GetAscentLine().GetBoundingRectangle();
            var desc = charRenderInfo.GetDescentLine().GetBoundingRectangle();

            var x1 = Math.Min(asc.GetX(), desc.GetX());
            var y1 = Math.Min(asc.GetY(), desc.GetY());
            var x2 = Math.Max(asc.GetX() + asc.GetWidth(), desc.GetX() + desc.GetWidth());
            var y2 = Math.Max(asc.GetY() + asc.GetHeight(), desc.GetY() + desc.GetHeight());

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        private static Rectangle UnionRects(IEnumerable<(string ch, Rectangle rect)> rects)
        {
            float x1 = float.PositiveInfinity, y1 = float.PositiveInfinity;
            float x2 = float.NegativeInfinity, y2 = float.NegativeInfinity;

            foreach (var (_, r) in rects)
            {
                x1 = Math.Min(x1, r.GetX());
                y1 = Math.Min(y1, r.GetY());
                x2 = Math.Max(x2, r.GetX() + r.GetWidth());
                y2 = Math.Max(y2, r.GetY() + r.GetHeight());
            }

            // If something went wrong, avoid NaNs in returned rectangles.
            if (float.IsInfinity(x1) || float.IsInfinity(y1) || float.IsInfinity(x2) || float.IsInfinity(y2))
                return new Rectangle(0, 0, 0, 0);

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }
    }
}
