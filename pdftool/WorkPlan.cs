namespace PdfTool;

public sealed record OverlayPlan(
    string Name,
    IReadOnlyList<int> Pages,
    PlacementSpec Placement,
    IReadOnlyList<PrimitiveSpec> Primitives
);

public static class WorkPlanBuilder
{
    /// <summary>
    /// Builds an execution plan without reading PDF contents (except totalPages is required).
    /// Performs:
    /// - pages syntax + resolution (last)
    /// - overlay validation
    /// </summary>
    public static IReadOnlyList<OverlayPlan> Build(DocumentSpec spec, int totalPages)
    {
        if (spec is null) throw new ArgumentNullException(nameof(spec));
        if (spec.Overlays is null || spec.Overlays.Count == 0)
            throw new FormatException("JSON must contain non-empty 'overlays' array");

        var plans = new List<OverlayPlan>(spec.Overlays.Count);

        foreach (var ov in spec.Overlays)
        {
            // Validate overlay (placement + primitives)
            JsonSpecValidator.ValidateOverlay(ov);

            // Resolve pages
            var pages = PagesRangeParser.Resolve(ov.Pages, totalPages);

            plans.Add(new OverlayPlan(
                Name: ov.Name,
                Pages: pages,
                Placement: ov.Placement,
                Primitives: ov.Primitives
            ));
        }

        return plans;
    }
}
