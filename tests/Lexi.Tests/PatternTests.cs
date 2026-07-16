using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Lexi.Tests;

[ExcludeFromCodeCoverage]
public sealed class PatternTests
{
    [Fact]
    public void New_StringOverload_MatchesAtOffset()
    {
        var pattern = Pattern.New("abc", 7);

        var symbol = pattern.Match("xxabc", 2);

        Assert.True(symbol.IsMatch);
        Assert.Equal(2, symbol.Offset);
        Assert.Equal(3, symbol.Length);
        Assert.Equal(7u, symbol.TokenId);
    }

    [Fact]
    public void New_StringWithRegexOptions_HonoursOptions()
    {
        var pattern = Pattern.New("abc", 1, RegexOptions.IgnoreCase);

        var symbol = pattern.Match("ABC", 0);

        Assert.True(symbol.IsMatch);
        Assert.Equal(3, symbol.Length);
    }

    [Fact]
    public void New_RegexOverload_MatchesAtOffset()
    {
        var pattern = Pattern.New(CommonPatterns.Identifier(), 3);

        var symbol = pattern.Match("abc", 0);

        Assert.True(symbol.IsMatch);
        Assert.Equal(3u, symbol.TokenId);
    }

    [Fact]
    public void Match_WhenNoMatch_SetsNoMatchFlagAndZeroLengthAtOffset()
    {
        var pattern = Pattern.New("abc", 5);

        var symbol = pattern.Match("xyz", 1);

        Assert.False(symbol.IsMatch);
        Assert.Equal(1, symbol.Offset);
        Assert.Equal(0, symbol.Length);
        Assert.Equal(5u | Pattern.NoMatch, symbol.TokenId);
    }

    [Fact]
    public void New_NullRegex_Throws()
    {
        Regex? regex = null;

        _ = Assert.Throws<ArgumentNullException>(() => Pattern.New(regex!, 1));
    }

    [Fact]
    public void New_NullPatternString_Throws()
    {
        string? pattern = null;

        _ = Assert.Throws<ArgumentNullException>(() => Pattern.New(pattern!, 1));
    }

    [Theory]
    [InlineData(Pattern.NoMatch)]
    [InlineData(Pattern.EndOfSource)]
    [InlineData(Pattern.NoMatch + 1)]
    public void New_ReservedTokenId_Throws(uint reservedTokenId) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Pattern.New("a", reservedTokenId));

    [Fact]
    public void New_TokenIdJustBelowReserved_Succeeds()
    {
        var pattern = Pattern.New("a", Pattern.NoMatch - 1);

        var symbol = pattern.Match("a", 0);

        Assert.Equal(Pattern.NoMatch - 1, symbol.TokenId);
    }
}
