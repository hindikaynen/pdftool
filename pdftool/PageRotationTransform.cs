using iText.Kernel.Geom;
using iText.Kernel.Pdf;

namespace PdfTool;

internal static class PageRotationTransform
{
    public static int NormalizeRotation(int rotation)
    {
        var r = rotation % 360;
        if (r < 0) r += 360;
        if (r is not (0 or 90 or 180 or 270))
        {
            // PDFs should only use multiples of 90. Fallback to nearest.
            r = ((r + 45) / 90) * 90;
            r %= 360;
        }
        return r;
    }

    public static (double UnrotatedWidth, double UnrotatedHeight) GetUnrotatedSize(PdfPage page)
    {
        var ps = page.GetPageSize(); // unrotated MediaBox in user space
        return (ps.GetWidth(), ps.GetHeight());
    }

    public static (double ViewWidth, double ViewHeight) GetViewSize(double unrotatedWidth, double unrotatedHeight, int rotation)
    {
        rotation = NormalizeRotation(rotation);
        return rotation is 90 or 270
            ? (unrotatedHeight, unrotatedWidth)
            : (unrotatedWidth, unrotatedHeight);
    }

    /// <summary>
    /// Convert a point from PDF user space (unrotated page coordinate system) to the "view" coordinate system,
    /// i.e. how the user sees the rotated page on screen (origin bottom-left).
    ///
    /// NOTE: In PDF, page rotation (/Rotate) is defined as counter-clockwise.
    /// </summary>
    public static PointD UserToView(PointD pUser, double unrotatedWidth, double unrotatedHeight, int rotation)
    {
        rotation = NormalizeRotation(rotation);

        return rotation switch
        {
            0 => pUser,

            // 90 degrees counter-clockwise
            90 => new PointD(
                X: pUser.Y,
                Y: unrotatedWidth - pUser.X
            ),

            180 => new PointD(
                X: unrotatedWidth - pUser.X,
                Y: unrotatedHeight - pUser.Y
            ),

            // 270 degrees counter-clockwise (i.e. 90 CW)
            270 => new PointD(
                X: unrotatedHeight - pUser.Y,
                Y: pUser.X
            ),

            _ => pUser
        };
    }

    /// <summary>
    /// Convert a point from "view" coordinates (what user sees on rotated page) to PDF user space.
    /// </summary>
    public static PointD ViewToUser(PointD pView, double unrotatedWidth, double unrotatedHeight, int rotation)
    {
        rotation = NormalizeRotation(rotation);

        return rotation switch
        {
            0 => pView,

            // inverse of 90 CCW
            90 => new PointD(
                X: unrotatedWidth - pView.Y,
                Y: pView.X
            ),

            180 => new PointD(
                X: unrotatedWidth - pView.X,
                Y: unrotatedHeight - pView.Y
            ),

            // inverse of 270 CCW
            270 => new PointD(
                X: pView.Y,
                Y: unrotatedHeight - pView.X
            ),

            _ => pView
        };
    }

    /// <summary>
    /// Convert an axis-aligned rectangle in "view" coordinates into an axis-aligned rectangle in PDF user space.
    /// The input rectangle is defined by its bottom-left corner and width/height in view space.
    /// </summary>
    public static Rectangle ViewRectToUserRect(Rectangle viewRect, double unrotatedWidth, double unrotatedHeight, int rotation)
    {
        rotation = NormalizeRotation(rotation);

        var xv = (double)viewRect.GetX();
        var yv = (double)viewRect.GetY();
        var wv = (double)viewRect.GetWidth();
        var hv = (double)viewRect.GetHeight();

        return rotation switch
        {
            0 => new Rectangle((float)xv, (float)yv, (float)wv, (float)hv),

            90 => new Rectangle(
                (float)(unrotatedWidth - (yv + hv)),
                (float)xv,
                (float)hv,
                (float)wv
            ),

            180 => new Rectangle(
                (float)(unrotatedWidth - (xv + wv)),
                (float)(unrotatedHeight - (yv + hv)),
                (float)wv,
                (float)hv
            ),

            270 => new Rectangle(
                (float)yv,
                (float)(unrotatedHeight - (xv + wv)),
                (float)hv,
                (float)wv
            ),

            _ => new Rectangle((float)xv, (float)yv, (float)wv, (float)hv)
        };
    }

    /// <summary>
    /// Returns a PDF matrix [a b c d e f] that maps overlay primitive coordinates expressed in
    /// "view" space (what the user sees on the rotated page) into the annotation appearance's
    /// USER-space box coordinates.
    ///
    /// Why this works:
    /// - The viewer rotates the whole page (including annotations) by /Rotate (CCW).
    /// - If we draw the overlay in USER coords such that after that page-rotation it lands back
    ///   in the intended VIEW coords, the overlay looks upright to the user.
    /// - That is exactly the inverse mapping: USER = ViewToUser(VIEW).
    ///
    /// The provided userBoxWidth/userBoxHeight are the dimensions of the appearance box in USER space
    /// (i.e. the annotation rectangle width/height in unrotated PDF coords).
    /// </summary>
    public static float[] GetViewToUserMatrixForAppearanceBox(int rotation, double userBoxWidth, double userBoxHeight)
    {
        rotation = NormalizeRotation(rotation);

        return rotation switch
        {
            0 => [1f, 0f, 0f, 1f, 0f, 0f],

            // ViewToUser for 90 CCW: (x,y) -> (W - y, x)
            90 => [0f, 1f, -1f, 0f, (float)userBoxWidth, 0f],

            // ViewToUser for 180: (x,y) -> (W - x, H - y)
            180 => [-1f, 0f, 0f, -1f, (float)userBoxWidth, (float)userBoxHeight],

            // ViewToUser for 270 CCW: (x,y) -> (y, H - x)
            270 => [0f, -1f, 1f, 0f, 0f, (float)userBoxHeight],

            _ => [1f, 0f, 0f, 1f, 0f, 0f]
        };
    }
}
