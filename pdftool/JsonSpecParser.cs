using System.Text.Json;

namespace PdfTool;

public static class JsonSpecParser
{
    public static DocumentSpec Parse(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);

        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var spec = JsonSerializer.Deserialize<DocumentSpec>(json, opts)
                   ?? throw new FormatException("Invalid JSON: root is null");

        if (spec.Overlays is null || spec.Overlays.Count == 0)
            throw new FormatException("JSON must contain non-empty 'overlays' array");

        for (int i = 0; i < spec.Overlays.Count; i++)
        {
            var ov = spec.Overlays[i];
            if (string.IsNullOrWhiteSpace(ov.Name))
                throw new FormatException($"Overlay[{i}].name is required");
            if (string.IsNullOrWhiteSpace(ov.Pages))
                throw new FormatException($"Overlay[{i}].pages is required");
            if (ov.Placement is null)
                throw new FormatException($"Overlay[{i}].placement is required");
            if (ov.Primitives is null)
                throw new FormatException($"Overlay[{i}].primitives must be an array (can be empty)");
        }

        return spec;
    }
}
