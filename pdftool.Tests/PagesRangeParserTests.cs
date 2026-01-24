using System;
using NUnit.Framework;

namespace PdfTool.Tests;

[TestFixture]
public class PagesRangeParserTests
{
    [TestCase("all")]
    [TestCase("1")]
    [TestCase("last")]
    [TestCase("1-last")]
    [TestCase("1-3,5,7-last")]
    [TestCase(" 1 - 3 , 5 , 7 - last ")]
    public void ValidateSyntax_AcceptsValidExpressions(string range)
    {
        Assert.DoesNotThrow(() => PagesRangeParser.ValidateSyntax(range));
    }

    [TestCase("")]
    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("abc")]
    [TestCase("1-")]
    [TestCase("-3")]
    [TestCase("1--3")]
    [TestCase("last-3")]
    [TestCase("5-3")]
    [TestCase("1-2-3")]
    public void ValidateSyntax_RejectsInvalidExpressions(string range)
    {
        Assert.Throws<FormatException>(() => PagesRangeParser.ValidateSyntax(range));
    }

    [TestCase("1-last", true)]
    [TestCase("last", true)]
    [TestCase("all", false)]
    [TestCase("1-3,5", false)]
    public void UsesLastToken_Works(string range, bool expected)
    {
        Assert.That(PagesRangeParser.UsesLastToken(range), Is.EqualTo(expected));
    }

    [Test]
    public void Resolve_All_ReturnsAllPages()
    {
        var pages = PagesRangeParser.Resolve("all", totalPages: 5);
        Assert.That(pages, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public void Resolve_Last_Works()
    {
        var pages = PagesRangeParser.Resolve("last", totalPages: 7);
        Assert.That(pages, Is.EqualTo(new[] { 7 }));
    }

    [Test]
    public void Resolve_MixedRanges_DedupesAndSorts()
    {
        var pages = PagesRangeParser.Resolve("3,1-2,2,5-last", totalPages: 6);
        Assert.That(pages, Is.EqualTo(new[] { 1, 2, 3, 5, 6 }));
    }

    [Test]
    public void Resolve_OutOfBounds_Throws()
    {
        Assert.Throws<FormatException>(() => PagesRangeParser.Resolve("1-3,10", totalPages: 5));
    }

    [Test]
    public void Resolve_TotalPagesMustBePositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PagesRangeParser.Resolve("1", totalPages: 0));
    }
}
