using System.Text.Json;

namespace PdfTool;

public static class JsonSpecValidator
{
    public static void ValidateOverlay(OverlaySpec ov)
    {
        ValidatePlacement(ov.Name, ov.Placement);

        foreach (var prim in ov.Primitives)
            ValidatePrimitive(ov.Name, prim);
    }

    private static void ValidatePlacement(string name, PlacementSpec p)
    {
        if (p.Offset is not null && p.Offset.Length != 2)
            throw new FormatException($"Overlay '{name}': placement.offset must be [dx,dy]");

        if (p.Mode == PlacementMode.corner)
        {
            if (p.Corner is null)
                throw new FormatException($"Overlay '{name}': placement.corner is required for mode='corner'");
            if (!string.IsNullOrWhiteSpace(p.MarkerText))
                throw new FormatException($"Overlay '{name}': placement.text must not be set for mode='corner'");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(p.MarkerText))
                throw new FormatException($"Overlay '{name}': placement.text is required for mode='textMarker'");
            if (p.Corner is not null)
                throw new FormatException($"Overlay '{name}': placement.corner must not be set for mode='textMarker'");
        }
    }

    private static void ValidatePrimitive(string overlayName, PrimitiveSpec prim)
    {
        switch (prim)
        {
            case TextPrimitiveSpec t:
                ValidateText(overlayName, t);
                break;

            case RectPrimitiveSpec r:
                RequireLen(overlayName, "rect.rect", r.Rect, 4);
                if (r.Rect[2] < 0 || r.Rect[3] < 0)
                    throw new FormatException($"Overlay '{overlayName}': rect.w/h must be >= 0");
                if (r.StrokeWidth is not null && r.StrokeWidth < 0)
                    throw new FormatException($"Overlay '{overlayName}': rect.strokeWidth must be >= 0");
                if (r.CornerRadius is not null && r.CornerRadius < 0)
                    throw new FormatException($"Overlay '{overlayName}': rect.cornerRadius must be >= 0");
                break;

            case LinePrimitiveSpec l:
                RequireLen(overlayName, "line.from", l.From, 2);
                RequireLen(overlayName, "line.to", l.To, 2);
                if (l.StrokeWidth is not null && l.StrokeWidth < 0)
                    throw new FormatException($"Overlay '{overlayName}': line.strokeWidth must be >= 0");
                break;

            case ImagePrimitiveSpec im:
                RequireLen(overlayName, "image.rect", im.Rect, 4);
                if (im.Rect[2] < 0 || im.Rect[3] < 0)
                    throw new FormatException($"Overlay '{overlayName}': image.w/h must be >= 0");
                if (im.Data is null)
                    throw new FormatException($"Overlay '{overlayName}': image.data is required");
                if (string.IsNullOrWhiteSpace(im.Data.Mime))
                    throw new FormatException($"Overlay '{overlayName}': image.data.mime is required");
                if (!IsSupportedImageMime(im.Data.Mime))
                    throw new FormatException($"Overlay '{overlayName}': unsupported image mime '{im.Data.Mime}' (supported: image/png, image/jpeg)");
                if (string.IsNullOrWhiteSpace(im.Data.Base64))
                    throw new FormatException($"Overlay '{overlayName}': image.data.base64 is required");
                // Base64 syntax check (no heavy decoding)
                TryValidateBase64(im.Data.Base64, overlayName);
                break;

            case BarcodePrimitiveSpec bc:
                ValidateBarcode(overlayName, bc);
                break;

            default:
                throw new FormatException($"Overlay '{overlayName}': unknown primitive type '{prim.GetType().Name}'");
        }
    }

    private static void ValidateText(string overlayName, TextPrimitiveSpec t)
    {
        var hasAt = t.At is not null;
        var hasRect = t.Rect is not null;

        if (hasAt == hasRect) // both true or both false
            throw new FormatException($"Overlay '{overlayName}': text must define exactly one of 'at' or 'rect'");

        if (hasAt)
            RequireLen(overlayName, "text.at", t.At!, 2);

        if (hasRect)
        {
            RequireLen(overlayName, "text.rect", t.Rect!, 4);
            if (t.Rect![2] < 0 || t.Rect[3] < 0)
                throw new FormatException($"Overlay '{overlayName}': text.rect w/h must be >= 0");
        }

        if (t.Size is not null && t.Size <= 0)
            throw new FormatException($"Overlay '{overlayName}': text.size must be > 0");

        if (string.IsNullOrWhiteSpace(t.Value))
            throw new FormatException($"Overlay '{overlayName}': text.value must be non-empty");

        // Wrap makes sense for rect; allow but warn-less (we validate only)
        if (t.Wrap == true && !hasRect)
            throw new FormatException($"Overlay '{overlayName}': text.wrap=true requires text.rect");


if (t.VAlign is not null && !hasRect)
    throw new FormatException($"Overlay '{overlayName}': text.valign requires text.rect");

if (t.HAlign is not null && !hasRect)
    throw new FormatException($"Overlay '{overlayName}': text.halign requires text.rect");
    }

    private static void ValidateBarcode(string overlayName, BarcodePrimitiveSpec bc)
    {
        RequireLen(overlayName, "barcode.rect", bc.Rect, 4);
        if (bc.Rect[2] < 0 || bc.Rect[3] < 0)
            throw new FormatException($"Overlay '{overlayName}': barcode.w/h must be >= 0");

        if (string.IsNullOrWhiteSpace(bc.Value))
            throw new FormatException($"Overlay '{overlayName}': barcode.value must be non-empty");

        if (bc.Options is null || bc.Options.Value.ValueKind == JsonValueKind.Undefined || bc.Options.Value.ValueKind == JsonValueKind.Null)
            return;

        if (bc.Options.Value.ValueKind != JsonValueKind.Object)
            throw new FormatException($"Overlay '{overlayName}': barcode.options must be an object");

        var opt = bc.Options.Value;

        if (bc.Kind == BarcodeKind.qr)
        {
            if (opt.TryGetProperty("ecLevel", out var ec))
            {
                if (ec.ValueKind != JsonValueKind.String)
                    throw new FormatException($"Overlay '{overlayName}': barcode.options.ecLevel must be a string");
                var s = ec.GetString();
                if (s is null || (s != "L" && s != "M" && s != "Q" && s != "H"))
                    throw new FormatException($"Overlay '{overlayName}': barcode.options.ecLevel must be one of L,M,Q,H");
            }
        }
        else if (bc.Kind == BarcodeKind.code128)
        {
            if (opt.TryGetProperty("showText", out var st))
            {
                if (st.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    throw new FormatException($"Overlay '{overlayName}': barcode.options.showText must be boolean");
            }
        }
    }

    private static bool IsSupportedImageMime(string mime) =>
        string.Equals(mime, "image/png", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(mime, "image/jpeg", StringComparison.OrdinalIgnoreCase);

    private static void TryValidateBase64(string b64, string overlayName)
    {
        try
        {
            // This checks syntax and padding; result is discarded
            _ = Convert.FromBase64String(b64);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Overlay '{overlayName}': image.data.base64 is not valid base64 ({ex.GetType().Name})");
        }
    }

    private static void RequireLen(string overlay, string field, double[] arr, int len)
    {
        if (arr.Length != len)
            throw new FormatException($"Overlay '{overlay}': {field} must be an array of length {len}");
    }
}
