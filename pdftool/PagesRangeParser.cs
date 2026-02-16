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
            // 1) last-3 is impossible to be valid for any last >= 1
            if (IsLast(aTok) && int.TryParse(bTok, out var b) && b >= 1)
                throw new FormatException($"Invalid segment '{part}': 'last' cannot be range start with a fixed smaller end");

            // 2) numeric descending range
            if (int.TryParse(aTok, out var aNum) && int.TryParse(bTok, out var bNum) && aNum > bNum)
                throw new FormatException($"Invalid segment '{part}': from > to");
        }
    }

    /// <summary>
    /// Resolves pages range into a sorted distinct list of page numbers (1-based), given total pages.
    /// This method DOES require knowing the PDF total page count to resolve "last".
    /// </summary>
    public static IReadOnlyList<int> Resolve(string range, int totalPages)
    {
        if (totalPages <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalPages), "totalPages must be > 0");

        ValidateSyntax(range);

        var cleaned = Regex.Replace(range, @"\s+", "");

        var set = new SortedSet<int>();

        if (string.Equals(cleaned, "all", StringComparison.OrdinalIgnoreCase))
        {
            for (int i = 1; i <= totalPages; i++) set.Add(i);
            return set.ToList();
        }

        foreach (var part in cleaned.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var dashCount = part.Count(c => c == '-');
            if (dashCount == 0)
            {
                var p = ResolveToken(part, totalPages);
                set.Add(p);
                continue;
            }

            // dashCount == 1 enforced by ValidateSyntax
            var idx = part.IndexOf('-');
            var aTok = part.Substring(0, idx);
            var bTok = part.Substring(idx + 1);

            var a = ResolveToken(aTok, totalPages);
            var b = ResolveToken(bTok, totalPages);

            if (a > b)
                throw new FormatException($"Invalid pages '{range}': resolved range '{part}' has from > to ({a}>{b})");

            for (int p = a; p <= b; p++)
                set.Add(p);
        }

        return set.ToList();
    }

    public static bool UsesLastToken(string range)
    {
        if (string.IsNullOrWhiteSpace(range)) return false;
        return Regex.IsMatch(range, @"\blast\b", RegexOptions.IgnoreCase);
    }

    private static int ResolveToken(string token, int totalPages)
    {
        if (IsLast(token)) return totalPages;

        if (!int.TryParse(token, out var n))
            throw new FormatException($"Invalid page token: '{token}'");

        return n;
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
