using System.Diagnostics.CodeAnalysis;

namespace Lexi.Tests;

[ExcludeFromCodeCoverage]
public sealed class StringLiteralTests(Lexer lexer)
{
    private readonly Lexer lexer = lexer
        ?? throw new ArgumentNullException(nameof(lexer));

    [Theory]
    [InlineData(@"""hello, world.""", "hello, world.", true)]
    [InlineData(@"""hello, \""world.\""""", @"hello, \""world.\""", true)]
    [InlineData(@"""hello, \""world.\"""" ""this is string two""", @"hello, \""world.\""", true)]
    public void StringLiterals(string source, string expectedSymbol, bool expectedSuccess)
    {
        var result = lexer.NextMatch(source);
        Assert.Equal(expectedSuccess, result.Symbol.IsMatch);
        if (expectedSuccess)
        {
            Assert.Equal(expectedSymbol, result.Source.ReadSymbol(in result.Symbol)[1..^1]);
        }
    }
}
