using System;
using Xunit;

namespace PdfTool.Tests;

public class PlacementResolverTests
{
    [Fact]
    public void ResolveCornerOriginVariantB_BottomLeft_NoOffset_AnchorsOverlayBottomLeft()
    {
        var p = PlacementResolver.ResolveCornerOrigin(
            pageWidth: 600,
            pageHeight: 800,
            corner: PageCorner.bottomLeft,
            offset: null,
            overlayMinX: 0,
            overlayMinY: 0,
            overlayMaxX: 100,
            overlayMaxY: 50);

        // Target point for bottomLeft with no offset is (0,0),
        // and overlay's bottom-left corner is (minX,minY)=(0,0),
        // so origin must be (0,0).
        Assert.Equal(0, p.X);
        Assert.Equal(0, p.Y);
    }

    [Fact]
    public void ResolveCornerOriginVariantB_TopLeft_WithOffset_AnchorsOverlayTopLeft()
    {
        var p = PlacementResolver.ResolveCornerOrigin(
            pageWidth: 600,
            pageHeight: 800,
            corner: PageCorner.topLeft,
            offset: new[] { 10d, 20d }, // inward: right=10, down=20
            overlayMinX: 0,
            overlayMinY: 0,
            overlayMaxX: 100,
            overlayMaxY: 50);

        // Target point for topLeft is (dx, pageHeight-dy) = (10, 780).
        // Overlay top-left corner in local coords is (minX, maxY) = (0, 50).
        // origin = target - overlayCorner = (10, 730).
        Assert.Equal(10, p.X);
        Assert.Equal(730, p.Y);
    }

    [Fact]
    public void ResolveCornerOriginVariantB_TopRight_WithOffset_AnchorsOverlayTopRight()
    {
        var p = PlacementResolver.ResolveCornerOrigin(
            pageWidth: 600,
            pageHeight: 800,
            corner: PageCorner.topRight,
            offset: new[] { 10d, 20d }, // inward: left=10, down=20
            overlayMinX: 0,
            overlayMinY: 0,
            overlayMaxX: 100,
            overlayMaxY: 50);

        // Target point for topRight is (pageWidth-dx, pageHeight-dy) = (590, 780).
        // Overlay top-right corner in local coords is (maxX, maxY) = (100, 50).
        // origin = (590-100, 780-50) = (490, 730).
        Assert.Equal(490, p.X);
        Assert.Equal(730, p.Y);
    }

    [Fact]
    public void ResolveCornerOriginVariantB_BottomRight_WithOffset_AnchorsOverlayBottomRight()
    {
        var p = PlacementResolver.ResolveCornerOrigin(
            pageWidth: 600,
            pageHeight: 800,
            corner: PageCorner.bottomRight,
            offset: new[] { 10d, 20d }, // inward: left=10, up=20
            overlayMinX: 0,
            overlayMinY: 0,
            overlayMaxX: 100,
            overlayMaxY: 50);

        // Target point for bottomRight is (pageWidth-dx, dy) = (590, 20).
        // Overlay bottom-right corner in local coords is (maxX, minY) = (100, 0).
        // origin = (590-100, 20-0) = (490, 20).
        Assert.Equal(490, p.X);
        Assert.Equal(20, p.Y);
    }

    [Fact]
    public void ResolveCornerOriginVariantB_OffsetMustBeLen2()
    {
        Assert.Throws<ArgumentException>(() =>
            PlacementResolver.ResolveCornerOrigin(
                pageWidth: 600,
                pageHeight: 800,
                corner: PageCorner.bottomRight,
                offset: new[] { 1d },
                overlayMinX: 0,
                overlayMinY: 0,
                overlayMaxX: 100,
                overlayMaxY: 50));
    }

    [Fact]
    public void ResolveCornerOriginVariantB_PageSizeMustBePositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlacementResolver.ResolveCornerOrigin(
                pageWidth: 0,
                pageHeight: 800,
                corner: PageCorner.bottomLeft,
                offset: null,
                overlayMinX: 0,
                overlayMinY: 0,
                overlayMaxX: 100,
                overlayMaxY: 50));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlacementResolver.ResolveCornerOrigin(
                pageWidth: 600,
                pageHeight: -1,
                corner: PageCorner.bottomLeft,
                offset: null,
                overlayMinX: 0,
                overlayMinY: 0,
                overlayMaxX: 100,
                overlayMaxY: 50));
    }
}
