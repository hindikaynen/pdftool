namespace PdfTool;

public readonly record struct PointD(double X, double Y);

public static class PlacementResolver
{
    /// <summary>
    /// Resolves origin point for corner placement.
    /// Coordinate system: PDF user space, origin at bottom-left.
    /// The returned origin is the selected page corner plus optional offset [dx,dy] in pt.
    /// </summary>
    public static PointD ResolveCornerOrigin(double pageWidth, double pageHeight, PageCorner corner, double[]? offset)
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

        var basePoint = corner switch
        {
            PageCorner.bottomLeft => new PointD(0, 0),
            PageCorner.bottomRight => new PointD(pageWidth, 0),
            PageCorner.topLeft => new PointD(0, pageHeight),
            PageCorner.topRight => new PointD(pageWidth, pageHeight),
            _ => throw new ArgumentOutOfRangeException(nameof(corner))
        };

        return new PointD(basePoint.X + dx, basePoint.Y + dy);
    }
}
