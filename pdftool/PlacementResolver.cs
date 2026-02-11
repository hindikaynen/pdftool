namespace PdfTool;

public readonly record struct PointD(double X, double Y);

public static class PlacementResolver
{
    /// <summary>
    /// **User-friendly coordinate system (TOP-LEFT, Y-down)**
    ///
    /// This project intentionally uses a top-left origin for JSON specs:
    /// - (0,0) is the TOP-LEFT of the (rotated) page as the user sees it.
    /// - X grows to the right.
    /// - Y grows down.
    ///
    /// Offsets are ALWAYS "inward" from the chosen page corner (in pt):
    /// - topLeft:     [right, down]
    /// - topRight:    [left,  down]
    /// - bottomLeft:  [right, up]
    /// - bottomRight: [left,  up]
    ///
    /// This method anchors the OVERLAY bounds to that corner point so callers can keep
    /// primitive coordinates natural (x>=0, y>=0) regardless of corner.
    ///
    /// Returns an origin in VIEW TOP-LEFT coordinates.
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
        // Note: VIEW origin is top-left, so "down" from top means increasing Y.
        var target = corner switch
        {
            PageCorner.topLeft => new PointD(dx, dy),
            PageCorner.topRight => new PointD(pageWidth - dx, dy),
            PageCorner.bottomLeft => new PointD(dx, pageHeight - dy),
            PageCorner.bottomRight => new PointD(pageWidth - dx, pageHeight - dy),
            _ => throw new ArgumentOutOfRangeException(nameof(corner))
        };

        // Overlay corner point in overlay-local coordinates.
        // In top-left coordinates, MinY is the visual TOP and MaxY is the visual BOTTOM.
        var overlayCorner = corner switch
        {
            PageCorner.topLeft => new PointD(overlayMinX, overlayMinY),
            PageCorner.topRight => new PointD(overlayMaxX, overlayMinY),
            PageCorner.bottomLeft => new PointD(overlayMinX, overlayMaxY),
            PageCorner.bottomRight => new PointD(overlayMaxX, overlayMaxY),
            _ => throw new ArgumentOutOfRangeException(nameof(corner))
        };

        // origin + overlayCorner = target
        return new PointD(target.X - overlayCorner.X, target.Y - overlayCorner.Y);
    }
}
