using System;
using NUnit.Framework;

namespace PdfTool.Tests;

[TestFixture]
public class PlacementResolverTests
{
    [Test]
    public void ResolveCornerOrigin_BottomLeft_NoOffset()
    {
        var p = PlacementResolver.ResolveCornerOrigin(600, 800, PageCorner.bottomLeft, offset: null);
        Assert.That(p.X, Is.EqualTo(0));
        Assert.That(p.Y, Is.EqualTo(0));
    }

    [Test]
    public void ResolveCornerOrigin_TopRight_NoOffset()
    {
        var p = PlacementResolver.ResolveCornerOrigin(600, 800, PageCorner.topRight, offset: null);
        Assert.That(p.X, Is.EqualTo(600));
        Assert.That(p.Y, Is.EqualTo(800));
    }

    [Test]
    public void ResolveCornerOrigin_TopLeft_WithOffset()
    {
        var p = PlacementResolver.ResolveCornerOrigin(600, 800, PageCorner.topLeft, offset: new[] { 10d, -20d });
        Assert.That(p.X, Is.EqualTo(10));
        Assert.That(p.Y, Is.EqualTo(780));
    }

    [Test]
    public void ResolveCornerOrigin_OffsetMustBeLen2()
    {
        Assert.Throws<ArgumentException>(() =>
            PlacementResolver.ResolveCornerOrigin(600, 800, PageCorner.bottomRight, offset: new[] { 1d }));
    }

    [Test]
    public void ResolveCornerOrigin_PageSizeMustBePositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlacementResolver.ResolveCornerOrigin(0, 800, PageCorner.bottomLeft, null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlacementResolver.ResolveCornerOrigin(600, -1, PageCorner.bottomLeft, null));
    }
}
