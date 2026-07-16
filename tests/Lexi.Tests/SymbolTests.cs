using System.Diagnostics.CodeAnalysis;

namespace Lexi.Tests;

[ExcludeFromCodeCoverage]
public sealed class SymbolTests
{
    [Fact]
    public void Constructor_NegativeOffset_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Symbol(-1, 0, 0));

    [Fact]
    public void Constructor_NegativeLength_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Symbol(0, -1, 0));

    [Fact]
    public void Constructor_ZeroOffsetAndLength_Allowed()
    {
        var symbol = new Symbol(0, 0, 0);

        Assert.Equal(0, symbol.Offset);
        Assert.Equal(0, symbol.Length);
    }

    [Theory]
    [InlineData(42u, 42u, true)]
    [InlineData(42u, 43u, false)]
    public void Is_ComparesTokenId(uint tokenId, uint probe, bool expected)
    {
        var symbol = new Symbol(0, 1, tokenId);

        Assert.Equal(expected, symbol.Is(probe));
    }

    [Fact]
    public void IsMatch_TrueForPositiveLengthNonReservedToken()
    {
        var symbol = new Symbol(0, 3, 1);

        Assert.True(symbol.IsMatch);
    }

    [Fact]
    public void IsMatch_FalseWhenNoMatchFlagSet()
    {
        var symbol = new Symbol(0, 3, 1 | Pattern.NoMatch);

        Assert.False(symbol.IsMatch);
    }

    [Fact]
    public void IsMatch_FalseWhenZeroLength()
    {
        var symbol = new Symbol(0, 0, 1);

        Assert.False(symbol.IsMatch);
    }

    [Fact]
    public void IsMatch_FalseWhenEndOfSource()
    {
        var symbol = new Symbol(0, 3, Pattern.EndOfSource);

        Assert.False(symbol.IsMatch);
    }

    [Fact]
    public void IsEndOfSource_TrueWhenFlagSet()
    {
        var symbol = new Symbol(0, 0, Pattern.EndOfSource);

        Assert.True(symbol.IsEndOfSource);
    }

    [Fact]
    public void IsEndOfSource_FalseForOrdinaryToken()
    {
        var symbol = new Symbol(0, 1, 1);

        Assert.False(symbol.IsEndOfSource);
    }
}
