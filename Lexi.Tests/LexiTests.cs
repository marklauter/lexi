using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

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

[ExcludeFromCodeCoverage]
public sealed class LexiTests(Lexer lexer)
{
    private readonly Lexer lexer = lexer
        ?? throw new ArgumentNullException(nameof(lexer));

    [Theory]
    [InlineData("1", TestToken.IntegerLiteral)]
    [InlineData("-1", TestToken.IntegerLiteral)]
    [InlineData("10", TestToken.IntegerLiteral)]
    [InlineData("-10", TestToken.IntegerLiteral)]
    [InlineData("1.0", TestToken.FloatingPointLiteral)]
    [InlineData("0.1", TestToken.FloatingPointLiteral)]
    [InlineData("123.456", TestToken.FloatingPointLiteral)]
    [InlineData("-123.456", TestToken.FloatingPointLiteral)]
    [InlineData("+", TestToken.AdditionOperator)]
    [InlineData("-", TestToken.SubtractionOperator)]
    [InlineData("*", TestToken.MultiplicationOperator)]
    [InlineData("/", TestToken.DivisionOperator)]
    [InlineData("%", TestToken.ModulusOperator)]
    [InlineData("<", TestToken.GreaterThanOperator)]
    [InlineData("<=", TestToken.GreaterThanOrEqualOperator)]
    [InlineData("\"hello\"", TestToken.StringLiteral)]
    public void ReadsSymbol(string source, TestToken expectedId)
    {
        var result = lexer.NextMatch(source);
        Assert.Equal((int)expectedId, result.Symbol.TokenId);
        Assert.Equal(source, result.Source.ReadSymbol(in result.Symbol));
    }

    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "unit test")]
    [Theory]
    [InlineData("1 -1 10 1.0 0.1 + -", new TestToken[] {
        TestToken.IntegerLiteral,
        TestToken.IntegerLiteral,
        TestToken.IntegerLiteral,
        TestToken.FloatingPointLiteral,
        TestToken.FloatingPointLiteral,
        TestToken.AdditionOperator,
        TestToken.SubtractionOperator })]
    public void ReadToEndOfSource(string source, TestToken[] expectedId)
    {
        var symbols = source.Split(' ');
        var nextSource = new Source(source);
        for (var i = 0; i < expectedId.Length; ++i)
        {
            var result = lexer.NextMatch(nextSource);
            Assert.Equal((int)expectedId[i], result.Symbol.TokenId);
            var symbol = result.Symbol;
            Assert.Equal(symbols[i], result.Source.ReadSymbol(in symbol));
            nextSource = result.Source;
        }
    }

    [Fact]
    public void LexOrder()
    {
        var source = "from Address where Street startswith \"Cypress\" and (City = \"Tampa\" or City = \"Miami\")";
        var lexer = VocabularyBuilder
            .Create(RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
            .Match($"{nameof(TokenIds.FROM)}", TokenIds.FROM)
            .Match($"{nameof(TokenIds.WHERE)}", TokenIds.WHERE)
            .Match($"{nameof(TokenIds.SKIP)}", TokenIds.SKIP)
            .Match($"{nameof(TokenIds.TAKE)}", TokenIds.TAKE)
            .Match("contains|c", TokenIds.CONTAINS)
            .Match("startswith|sw", TokenIds.STARTS_WITH)
            .Match("endswith|ew", TokenIds.ENDS_WITH)
            .Match(@"and|&&", TokenIds.LOGICAL_AND)
            .Match(@"or|\|\|", TokenIds.LOGICAL_OR)
            .Match(CommonPatterns.Identifier(), TokenIds.IDENTIFIER)
            .Match("true", TokenIds.TRUE)
            .Match("false", TokenIds.FALSE)
            .Match(CommonPatterns.IntegerLiteral(), TokenIds.INTEGER_LITERAL)
            .Match(CommonPatterns.FloatingPointLiteral(), TokenIds.FLOATING_POINT_LITERAL)
            .Match(CommonPatterns.ScientificNotationLiteral(), TokenIds.SCIENTIFIC_NOTATION_LITERAL)
            .Match(CommonPatterns.QuotedStringLiteral(), TokenIds.STRING_LITERAL)
            .Match(CommonPatterns.CharacterLiteral(), TokenIds.CHAR_LITERAL)
            .Match(@"\(", TokenIds.OPEN_PARENTHESIS)
            .Match(@"\)", TokenIds.CLOSE_PARENTHESIS)
            .Match("=|==", TokenIds.EQUAL)
            .Match(">", TokenIds.GREATER_THAN)
            .Match(">=", TokenIds.GREATER_THAN_OR_EQUAL)
            .Match("<", TokenIds.LESS_THAN)
            .Match("<=", TokenIds.LESS_THAN_OR_EQUAL)
            .Match("!=", TokenIds.NOT_EQUAL)
            .Build();

        var match = lexer.NextMatch(source);
        Assert.Equal(TokenIds.FROM, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.IDENTIFIER, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.WHERE, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.IDENTIFIER, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.STARTS_WITH, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.STRING_LITERAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.LOGICAL_AND, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.OPEN_PARENTHESIS, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.IDENTIFIER, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.EQUAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.STRING_LITERAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.LOGICAL_OR, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.IDENTIFIER, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.EQUAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.STRING_LITERAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.CLOSE_PARENTHESIS, match.Symbol.TokenId);
    }

    [Fact]
    public void LexOrder2()
    {
        var source = "(City = \"Tampa\" or City = \"Miami\")";

        var lexer = VocabularyBuilder
            .Create(RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
            .Match("(?:contains|c)", TokenIds.CONTAINS)
            .Match("and|&&", TokenIds.LOGICAL_AND)
            .Match(@"or|\|\|", TokenIds.LOGICAL_OR)
            .Match(CommonPatterns.Identifier(), TokenIds.IDENTIFIER)
            .Match(CommonPatterns.QuotedStringLiteral(), TokenIds.STRING_LITERAL)
            .Match(CommonPatterns.CharacterLiteral(), TokenIds.CHAR_LITERAL)
            .Match(@"\(", TokenIds.OPEN_PARENTHESIS)
            .Match(@"\)", TokenIds.CLOSE_PARENTHESIS)
            .Match("=|==", TokenIds.EQUAL)
            .Build();

        var match = lexer.NextMatch(source);
        Assert.Equal(TokenIds.OPEN_PARENTHESIS, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.IDENTIFIER, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.EQUAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.STRING_LITERAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.LOGICAL_OR, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.IDENTIFIER, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.EQUAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.STRING_LITERAL, match.Symbol.TokenId);

        match = lexer.NextMatch(match);
        Assert.Equal(TokenIds.CLOSE_PARENTHESIS, match.Symbol.TokenId);
    }

    internal sealed class TokenIds
    {
        // literals
        public const int FALSE = 0;
        public const int TRUE = 1;
        public const int FLOATING_POINT_LITERAL = 2;
        public const int INTEGER_LITERAL = 3;
        public const int SCIENTIFIC_NOTATION_LITERAL = 4;
        public const int STRING_LITERAL = 5;
        public const int CHAR_LITERAL = 6;

        // delimiters
        public const int OPEN_PARENTHESIS = '('; // 40
        public const int CLOSE_PARENTHESIS = ')'; // 41

        // comparison operators
        public const int EQUAL = '='; // 61
        public const int GREATER_THAN = '>'; // 62
        public const int LESS_THAN = '<'; // 60
        public const int NOT_EQUAL = 400;
        public const int LESS_THAN_OR_EQUAL = 401;
        public const int GREATER_THAN_OR_EQUAL = 402;
        public const int STARTS_WITH = 405;
        public const int ENDS_WITH = 406;
        public const int CONTAINS = 407;

        // logical operators
        public const int LOGICAL_AND = 403;
        public const int LOGICAL_OR = 404;

        // names
        public const int IDENTIFIER = 500;

        // keywords
        public const int FROM = 300;
        public const int WHERE = 301;
        public const int SKIP = 302;
        public const int TAKE = 303;
    }
}
