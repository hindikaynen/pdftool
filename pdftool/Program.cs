using System.CommandLine;
using System.Globalization;
using iText.Kernel.Pdf;

namespace PdfTool;

internal static class Program
{
    public static int Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        var root = new RootCommand("pdftool - PDF overlay tool (JSON spec)");

        // validate
        var validateJson = new Option<FileInfo>("--json")
        {
            Description = "Overlays JSON path",
            Required = true
        };

        var validateCmd = new Command("validate", "Validate overlays JSON (no PDF required)");
        validateCmd.Options.Add(validateJson);

        validateCmd.SetAction(parseResult =>
        {
            var jsonFile = parseResult.GetValue(validateJson)!;
            try
            {
                RunValidate(jsonFile);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        });

        root.Subcommands.Add(validateCmd);

        // apply
        var inOpt = new Option<FileInfo>("--in")
        {
            Description = "Input PDF path",
            Required = true
        };

        var outOpt = new Option<FileInfo>("--out")
        {
            Description = "Output PDF path",
            Required = true
        };

        var applyJson = new Option<FileInfo>("--json")
        {
            Description = "Overlays JSON path",
            Required = true
        };

        var applyCmd = new Command("apply", "Apply overlays to a PDF");
        applyCmd.Options.Add(inOpt);
        applyCmd.Options.Add(outOpt);
        applyCmd.Options.Add(applyJson);

        applyCmd.SetAction(parseResult =>
        {
            var inPdf = parseResult.GetValue(inOpt)!;
            var outPdf = parseResult.GetValue(outOpt)!;
            var jsonFile = parseResult.GetValue(applyJson)!;

            RunApply(inPdf, outPdf, jsonFile);
        });

        root.Subcommands.Add(applyCmd);

        return root.Parse(args).Invoke();
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
    }

    private static void RunApply(FileInfo inPdf, FileInfo outPdf, FileInfo jsonFile)
    {
        if (!inPdf.Exists)
            throw new FileNotFoundException("Input PDF not found", inPdf.FullName);
        if (!jsonFile.Exists)
            throw new FileNotFoundException("JSON not found", jsonFile.FullName);

        var spec = JsonSpecParser.Parse(jsonFile.FullName);

        using var reader = new PdfReader(inPdf.FullName);
        using var writer = new PdfWriter(outPdf.FullName);
        using var pdf = new PdfDocument(reader, writer);
        PdfApplyEngine.Apply(pdf, spec);
    }
}
