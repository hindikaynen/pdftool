using System;
using Xunit;

namespace PdfTool.Tests;

public class PagesRangeParserTests
{
    [Theory]
    [InlineData("all")]
    [InlineData("1")]
    [InlineData("last")]
    [InlineData("1-last")]
    [InlineData("1-3,5,7-last")]
    [InlineData(" 1 - 3 , 5 , 7 - last ")]
    public void ValidateSyntax_AcceptsValidExpressions(string range)
    {
        PagesRangeParser.ValidateSyntax(range); // Should not throw
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("1-")]
    [InlineData("-3")]
    [InlineData("1--3")]
    [InlineData("last-3")]
    [InlineData("5-3")]
    [InlineData("1-2-3")]
    public void ValidateSyntax_RejectsInvalidExpressions(string range)
    {
        Assert.Throws<FormatException>(() => PagesRangeParser.ValidateSyntax(range));
    }

    [Theory]
    [InlineData("1-last", true)]
    [InlineData("last", true)]
    [InlineData("all", false)]
    [InlineData("1-3,5", false)]
    public void UsesLastToken_Works(string range, bool expected)
    {
        Assert.Equal(expected, PagesRangeParser.UsesLastToken(range));
    }

    [Fact]
    public void Resolve_All_ReturnsAllPages()
    {
        var pages = PagesRangeParser.Resolve("all", totalPages: 5);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, pages);
    }

    [Fact]
    public void Resolve_Last_Works()
    {
        var pages = PagesRangeParser.Resolve("last", totalPages: 7);
        Assert.Equal(new[] { 7 }, pages);
    }

    [Fact]
    public void Resolve_MixedRanges_DedupesAndSorts()
    {
        var pages = PagesRangeParser.Resolve("3,1-2,2,5-last", totalPages: 6);
        Assert.Equal(new[] { 1, 2, 3, 5, 6 }, pages);
    }

    [Fact]
    public void Resolve_OutOfBounds_Throws()
    {
        Assert.Throws<FormatException>(() => PagesRangeParser.Resolve("1-3,10", totalPages: 5));
    }

    [Fact]
    public void Resolve_TotalPagesMustBePositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PagesRangeParser.Resolve("1", totalPages: 0));
    }
}
