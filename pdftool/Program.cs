using System.CommandLine;
using System.Globalization;

namespace PdfTool;

internal static class Program
{
    public static int Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        var root = new RootCommand("pdftool - PDF overlay tool (JSON spec)");

        var validateCmd = new Command("validate", "Validate overlays JSON (no PDF required)")
        {
            new Option<FileInfo>("--json", "Overlays JSON path") { IsRequired = true }
        };

        validateCmd.SetHandler((FileInfo jsonFile) =>
        {
            RunValidate(jsonFile);
        }, validateCmd.Options[0] as Option<FileInfo>);

        root.AddCommand(validateCmd);

        return root.Invoke(args);
    }

    private static void RunValidate(FileInfo jsonFile)
    {
        if (!jsonFile.Exists)
            throw new FileNotFoundException("JSON not found", jsonFile.FullName);

        var spec = JsonSpecParser.Parse(jsonFile.FullName);

        foreach (var ov in spec.Overlays)
            PagesRangeParser.ValidateSyntax(ov.Pages);

        foreach (var ov in spec.Overlays)
            JsonSpecValidator.ValidateOverlay(ov);

        Console.WriteLine("JSON is valid.");
        Console.WriteLine($"Overlays: {spec.Overlays.Count}");
        Console.WriteLine($"Uses 'last': {spec.Overlays.Any(o => PagesRangeParser.UsesLastToken(o.Pages))}");
    }
}
