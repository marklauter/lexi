using System.Diagnostics.CodeAnalysis;

namespace Lexi.Tests;

[ExcludeFromCodeCoverage]
public sealed class LexerTests
{
    private const uint Word = 1;
    private const uint Number = 2;
    private const uint Space = 3;
    private const uint NewLine = 4;

    [Fact]
    public void Constructor_NullMatchPatterns_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new Lexer(null!, []));

    [Fact]
    public void Constructor_NullIgnorePatterns_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new Lexer([], null!));

    [Fact]
    public void NextMatch_StringOverload_MatchesFromOffsetZero()
    {
        var lexer = new Lexer([Pattern.New(@"[a-z]+", Word)], []);

        var match = lexer.NextMatch("abc");

        Assert.Equal(Word, match.Symbol.TokenId);
        Assert.Equal(0, match.Symbol.Offset);
        Assert.Equal("abc", match.Source.ReadSymbol(in match.Symbol));
    }

    [Fact]
    public void NextMatch_StringOverload_NullSource_Throws()
    {
        var lexer = new Lexer([Pattern.New(@"[a-z]+", Word)], []);

        _ = Assert.Throws<ArgumentNullException>(() => lexer.NextMatch(null!));
    }

    [Fact]
    public void NextMatch_AtEndOfSource_ReturnsEndOfSourceSymbol()
    {
        var lexer = new Lexer([Pattern.New(@"[a-z]+", Word)], []);

        var match = lexer.NextMatch(new Source("abc", 3));

        Assert.True(match.Symbol.IsEndOfSource);
        Assert.Equal(Pattern.EndOfSource, match.Symbol.TokenId);
    }

    [Fact]
    public void NextMatch_LexingReachesEndOfSource()
    {
        var lexer = new Lexer([Pattern.New(@"[a-z]+", Word)], []);

        var first = lexer.NextMatch("abc");
        var second = lexer.NextMatch(first);

        Assert.Equal(Word, first.Symbol.TokenId);
        Assert.True(second.Symbol.IsEndOfSource);
    }

    [Fact]
    public void NextMatch_NoPatternMatches_ReturnsNoMatchAndDoesNotAdvance()
    {
        var lexer = new Lexer([Pattern.New(@"[a-z]+", Word)], []);

        var match = lexer.NextMatch("123");

        Assert.False(match.Symbol.IsMatch);
        Assert.False(match.Symbol.IsEndOfSource);
        Assert.Equal(0, match.Source.Offset);
    }

    [Fact]
    public void NextMatch_LongerMatchWins_RegardlessOfPatternOrder()
    {
        var shortFirst = new Lexer([Pattern.New(@"a", Word), Pattern.New(@"ab", Number)], []);
        var longFirst = new Lexer([Pattern.New(@"ab", Number), Pattern.New(@"a", Word)], []);

        Assert.Equal(Number, shortFirst.NextMatch("ab").Symbol.TokenId);
        Assert.Equal(Number, longFirst.NextMatch("ab").Symbol.TokenId);
    }

    [Fact]
    public void NextMatch_EqualLengthTie_LowestIndexWins()
    {
        var lexer = new Lexer([Pattern.New(@"ab", Word), Pattern.New(@"a.", Number)], []);

        Assert.Equal(Word, lexer.NextMatch("ab").Symbol.TokenId);
    }

    [Fact]
    public void NextMatch_IgnorePatternsAdvanceOffsetBeforeMatching()
    {
        var lexer = new Lexer(
            [Pattern.New(@"[a-z]+", Word)],
            [Pattern.New(@" +", Space), Pattern.New(@"\r?\n", NewLine)]);

        var match = lexer.NextMatch("   abc");

        Assert.Equal(Word, match.Symbol.TokenId);
        Assert.Equal(3, match.Symbol.Offset);
        Assert.Equal("abc", match.Source.ReadSymbol(in match.Symbol));
    }

    [Fact]
    public void NextMatch_TrailingIgnoredContent_ReturnsEndOfSource()
    {
        var lexer = new Lexer([Pattern.New(@"[a-z]+", Word)], [Pattern.New(@" +", Space)]);

        var first = lexer.NextMatch("abc ");
        var second = lexer.NextMatch(first);

        Assert.Equal(Word, first.Symbol.TokenId);
        Assert.True(second.Symbol.IsEndOfSource);
    }

    [Fact]
    public void NextMatch_MatchResultOverload_ContinuesFromPreviousSource()
    {
        var lexer = new Lexer([Pattern.New(@"[a-z]+", Word), Pattern.New(@"\d+", Number)], [Pattern.New(@" +", Space)]);

        var first = lexer.NextMatch("abc 123");
        var second = lexer.NextMatch(first);

        Assert.Equal(Word, first.Symbol.TokenId);
        Assert.Equal(Number, second.Symbol.TokenId);
        Assert.Equal("123", second.Source.ReadSymbol(in second.Symbol));
    }
}
