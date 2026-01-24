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
        var validateCmd = new Command("validate", "Validate overlays JSON (no PDF required)")
        {
            new Option<FileInfo>("--json", "Overlays JSON path") { IsRequired = true }
        };

        validateCmd.SetHandler((FileInfo jsonFile) =>
        {
            RunValidate(jsonFile);
        }, validateCmd.Options[0] as Option<FileInfo>);

        root.AddCommand(validateCmd);

        // apply (step 0/1: open pdf, validate json, build plan, write output as a copy)
        var applyCmd = new Command("apply", "Apply overlays to a PDF (currently: builds plan and rewrites PDF)")
        {
            new Option<FileInfo>("--in", "Input PDF path") { IsRequired = true },
            new Option<FileInfo>("--out", "Output PDF path") { IsRequired = true },
            new Option<FileInfo>("--json", "Overlays JSON path") { IsRequired = true }
        };

        applyCmd.SetHandler((FileInfo inPdf, FileInfo outPdf, FileInfo jsonFile) =>
        {
            RunApply(inPdf, outPdf, jsonFile);
        },
        applyCmd.Options[0] as Option<FileInfo>,
        applyCmd.Options[1] as Option<FileInfo>,
        applyCmd.Options[2] as Option<FileInfo>);

        root.AddCommand(applyCmd);

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

        var totalPages = pdf.GetNumberOfPages();

        // Build plan (resolves 'last' etc.)
        var plan = WorkPlanBuilder.Build(spec, totalPages);

        Console.WriteLine($"Plan built. Overlays: {plan.Count}, totalPages: {totalPages}");
        Console.WriteLine("NOTE: Rendering is not implemented yet. Output PDF is a rewritten copy of input.");
    }
}
