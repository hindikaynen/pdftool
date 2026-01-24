using System.Text.RegularExpressions;

namespace PdfTool;

public static class PagesRangeParser
{
    public static void ValidateSyntax(string range)
    {
        if (string.IsNullOrWhiteSpace(range))
            throw new FormatException("pages is empty");

        var cleaned = Regex.Replace(range, @"\s+", "");

        if (string.Equals(cleaned, "all", StringComparison.OrdinalIgnoreCase))
            return;

        var parts = cleaned.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            throw new FormatException($"Invalid pages: '{range}'");

        foreach (var part in parts)
        {
            // Disallow multiple '-' (e.g. "1--3" or "1-2-3") and missing sides ("-3", "1-")
            var dashCount = part.Count(c => c == '-');
            if (dashCount == 0)
            {
                ValidateToken(part);
                continue;
            }
            if (dashCount != 1)
                throw new FormatException($"Invalid range segment: '{part}'");

            var idx = part.IndexOf('-');
            if (idx <= 0 || idx >= part.Length - 1)
                throw new FormatException($"Invalid range segment: '{part}'");

            var aTok = part.Substring(0, idx);
            var bTok = part.Substring(idx + 1);

            ValidateToken(aTok);
            ValidateToken(bTok);

            // Static guards without PDF:
            if (IsLast(aTok) && int.TryParse(bTok, out var b) && b >= 1)
                throw new FormatException($"Invalid segment '{part}': 'last' cannot be range start with a fixed smaller end");

            if (int.TryParse(aTok, out var aNum) && int.TryParse(bTok, out var bNum) && aNum > bNum)
                throw new FormatException($"Invalid segment '{part}': from > to");
        }
    }

    public static bool UsesLastToken(string range)
    {
        if (string.IsNullOrWhiteSpace(range)) return false;
        return Regex.IsMatch(range, @"\blast\b", RegexOptions.IgnoreCase);
    }

    private static void ValidateToken(string token)
    {
        if (IsLast(token)) return;

        if (!int.TryParse(token, out var n))
            throw new FormatException($"Invalid page token: '{token}'");

        if (n <= 0)
            throw new FormatException($"Page number must be >= 1: '{token}'");
    }

    private static bool IsLast(string token) =>
        string.Equals(token, "last", StringComparison.OrdinalIgnoreCase);
}
