using Math.Parsing;
using Math.Parsing.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace Math.Parsing.Tests;

[ExcludeFromCodeCoverage]
public sealed class MathParserErrorTests(Parser parser)
{
    private readonly Parser parser = parser ?? throw new ArgumentNullException(nameof(parser));

    [Fact]
    public void Constructor_NullLexer_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new Parser(null!));

    [Fact]
    public void Parse_NullSource_Throws() =>
        Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));

    [Theory]
    [InlineData("+")]
    [InlineData("*")]
    [InlineData("/")]
    [InlineData("%")]
    public void Parse_OperatorAtValuePosition_ThrowsUnexpectedToken(string source) =>
        Assert.Throws<UnexpectedTokenException>(() => parser.Parse(source));

    [Theory]
    [InlineData("(1")]
    [InlineData("(1 + 2")]
    public void Parse_UnclosedParenthesis_ThrowsUnexpectedEndOfSource(string source) =>
        Assert.Throws<UnexpectedEndOfSourceException>(() => parser.Parse(source));

    [Theory]
    [InlineData("(1 2)")]
    [InlineData("(1 + 2 3)")]
    public void Parse_UnexpectedTokenWhereCloseParenExpected_ThrowsUnexpectedToken(string source) =>
        Assert.Throws<UnexpectedTokenException>(() => parser.Parse(source));
}
