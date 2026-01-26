using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfTool;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
)]
[JsonSerializable(typeof(DocumentSpec))]
internal partial class PdfToolJsonContext : JsonSerializerContext
{
}