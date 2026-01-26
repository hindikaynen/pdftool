namespace PdfTool;

public readonly record struct PointD(double X, double Y);

public static class PlacementResolver
{
    /// <summary>
    /// Offsets are ALWAYS "inward" from the chosen page corner (in pt):
    /// - topLeft:     [right, down]
    /// - topRight:    [left,  down]
    /// - bottomLeft:  [right, up]
    /// - bottomRight: [left,  up]
    ///
    /// This method also anchors the OVERLAY bounds to that corner point so that callers can
    /// keep primitive coordinates natural (x>=0, y>=0) regardless of corner.
    ///
    /// Returns an origin in PDF user space (bottom-left).
    /// When you later place primitives with (origin + localX, origin + localY), the chosen
    /// overlay corner will match the target point inside the page.
    /// </summary>
    public static PointD ResolveCornerOrigin(
        double pageWidth,
        double pageHeight,
        PageCorner corner,
        double[]? offset,
        double overlayMinX,
        double overlayMinY,
        double overlayMaxX,
        double overlayMaxY)
    {
        if (pageWidth <= 0) throw new ArgumentOutOfRangeException(nameof(pageWidth));
        if (pageHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pageHeight));

        var dx = 0d;
        var dy = 0d;

        if (offset is not null)
        {
            if (offset.Length != 2)
                throw new ArgumentException("offset must be [dx,dy]", nameof(offset));
            dx = offset[0];
            dy = offset[1];
        }

        // Target point inside the page, by corner, using "inward" semantics.
        // Note: PDF origin is bottom-left, so "down" from top means decreasing Y.
        var target = corner switch
        {
            PageCorner.topLeft => new PointD(dx, pageHeight - dy),
            PageCorner.topRight => new PointD(pageWidth - dx, pageHeight - dy),
            PageCorner.bottomLeft => new PointD(dx, dy),
            PageCorner.bottomRight => new PointD(pageWidth - dx, dy),
            _ => throw new ArgumentOutOfRangeException(nameof(corner))
        };

        // Overlay corner point in overlay-local coordinates.
        var overlayCorner = corner switch
        {
            PageCorner.topLeft => new PointD(overlayMinX, overlayMaxY),
            PageCorner.topRight => new PointD(overlayMaxX, overlayMaxY),
            PageCorner.bottomLeft => new PointD(overlayMinX, overlayMinY),
            PageCorner.bottomRight => new PointD(overlayMaxX, overlayMinY),
            _ => throw new ArgumentOutOfRangeException(nameof(corner))
        };

        // origin + overlayCorner = target
        return new PointD(target.X - overlayCorner.X, target.Y - overlayCorner.Y);
    }
}
