using Predicate.Parsing.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace Predicate.Parsing.Tests;

[ExcludeFromCodeCoverage]
public sealed class PredicateParserErrorTests(Parser parser)
{
    private readonly Parser parser = parser ?? throw new ArgumentNullException(nameof(parser));

    [Fact]
    public void Constructor_NullLexer_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new Parser(null!));

    [Fact]
    public void Parse_NullSource_Throws() =>
        Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));

    [Theory]
    [InlineData("")]
    [InlineData("from")]
    [InlineData("from T")]
    [InlineData("from T where")]
    [InlineData("from T where x")]
    [InlineData("from T where x =")]
    [InlineData("from T where (x = 1")]
    [InlineData("from T where x = 1 skip")]
    [InlineData("from T where x = 1 skip 5 take")]
    public void Parse_TruncatedSource_ThrowsUnexpectedEndOfSource(string source) =>
        Assert.Throws<UnexpectedEndOfSourceException>(() => parser.Parse(source));

    [Theory]
    [InlineData("nope")]                     // missing 'from'
    [InlineData("from 1")]                   // 'from' not followed by identifier
    [InlineData("from T nope")]              // missing 'where'
    [InlineData("from T where 1 = 1")]       // comparison must start with identifier or '('
    [InlineData("from T where x y")]         // missing comparison operator
    [InlineData("from T where x = )")]       // value position is not a literal
    [InlineData("from T where x = and")]     // logical operator at value position
    [InlineData("from T where (x = 1 y")]    // parenthetical not closed by ')'
    [InlineData("from T where x = 1 nope")]  // trailing token is neither skip nor take
    [InlineData("from T where x = 1 where")]  // trailing keyword is neither skip nor take
    [InlineData("from T where x = 1 skip 5 nope")]        // trailing token after skip
    [InlineData("from T where x = 1 skip 5 take 9 nope")] // trailing token after take
    [InlineData("from T where x = 1 skip x")] // skip value is not numeric
    [InlineData("from T where x = 1 take x")] // take value is not numeric
    public void Parse_UnexpectedToken_ThrowsUnexpectedToken(string source) =>
        Assert.Throws<UnexpectedTokenException>(() => parser.Parse(source));
}
