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
}
