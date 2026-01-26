using System;
using Xunit;

namespace PdfTool.Tests;

public class WorkPlanBuilderTests
{
    [Fact]
    public void Build_ResolvesLast_AndReturnsPages()
    {
        var spec = new DocumentSpec
        {
            Overlays =
            [
                new OverlaySpec
                {
                    Name = "A",
                    Pages = "2-last",
                    Placement = new PlacementSpec { Mode = PlacementMode.corner, Corner = PageCorner.topLeft },
                    Primitives = []
                }
            ]
        };
        var plan = WorkPlanBuilder.Build(spec, totalPages: 5);
        Assert.Single(plan);
        Assert.Equal(new[] { 2, 3, 4, 5 }, plan[0].Pages);
    }

    [Fact]
    public void Build_OutOfBoundsPages_Throws()
    {
        var spec = new DocumentSpec
        {
            Overlays =
            [
                new OverlaySpec
                {
                    Name = "A",
                    Pages = "10",
                    Placement = new PlacementSpec { Mode = PlacementMode.corner, Corner = PageCorner.topLeft },
                    Primitives = []
                }
            ]
        };
        Assert.Throws<FormatException>(() => WorkPlanBuilder.Build(spec, totalPages: 3));
    }

    [Fact]
    public void Build_InvalidOverlay_Throws()
    {
        var spec = new DocumentSpec
        {
            Overlays =
            [
                new OverlaySpec
                {
                    Name = "A",
                    Pages = "1",
                    // invalid: corner mode without corner value
                    Placement = new PlacementSpec { Mode = PlacementMode.corner },
                    Primitives = []
                }
            ]
        };
        Assert.Throws<FormatException>(() => WorkPlanBuilder.Build(spec, totalPages: 1));
    }
}
