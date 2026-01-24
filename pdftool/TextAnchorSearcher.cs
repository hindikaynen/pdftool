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

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT) return;

            var tri = (TextRenderInfo)data;
            var text = tri.GetText();
            if (string.IsNullOrEmpty(text)) return;

            // Minimal behavior: detect occurrences inside a single text render event.
            // This assumes the marker text is emitted as a contiguous chunk.
            var idx = 0;
            while (true)
            {
                idx = text.IndexOf(needle, idx, StringComparison.Ordinal);
                if (idx < 0) break;

                // We approximate bbox by the whole TextRenderInfo bbox.
                // This is sufficient for typical marker strings like "<<ANCHOR>>".
                var asc = tri.GetAscentLine().GetBoundingRectangle();
                var desc = tri.GetDescentLine().GetBoundingRectangle();

                var x = Math.Min(asc.GetX(), desc.GetX());
                var y = Math.Min(asc.GetY(), desc.GetY());
                var x2 = Math.Max(asc.GetX() + asc.GetWidth(), desc.GetX() + desc.GetWidth());
                var y2 = Math.Max(asc.GetY() + asc.GetHeight(), desc.GetY() + desc.GetHeight());

                Boxes.Add(new Rectangle(x, y, x2 - x, y2 - y));

                idx += needle.Length;
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new HashSet<EventType> { EventType.RENDER_TEXT };
        }
    }
}
