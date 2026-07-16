using System.Diagnostics.CodeAnalysis;

namespace Lexi.Tests;

[ExcludeFromCodeCoverage]
public sealed class SourceTests
{
    [Fact]
    public void Constructor_NullText_Throws() =>
        Assert.Throws<ArgumentNullException>(() => _ = new Source(null!));

    [Fact]
    public void Constructor_DefaultsOffsetToZero()
    {
        var source = new Source("abc");

        Assert.Equal(0, source.Offset);
    }

    [Fact]
    public void Constructor_NegativeOffset_ClampsToZero()
    {
        var source = new Source("abc", -5);

        Assert.Equal(0, source.Offset);
    }

    [Fact]
    public void Constructor_OffsetWithinBounds_Preserved()
    {
        var source = new Source("abc", 2);

        Assert.Equal(2, source.Offset);
    }

    [Fact]
    public void IsEndOfSource_TrueWhenOffsetAtLength()
    {
        var source = new Source("abc", 3);

        Assert.True(source.IsEndOfSource);
    }

    [Fact]
    public void IsEndOfSource_FalseWhenOffsetBeforeLength()
    {
        var source = new Source("abc", 2);

        Assert.False(source.IsEndOfSource);
    }

    [Fact]
    public void Remaining_ReturnsTextAfterOffset()
    {
        var source = new Source("abcdef", 2);

        Assert.Equal("cdef", source.Remaining());
    }

    [Fact]
    public void ToString_ReturnsFullText()
    {
        var source = new Source("abcdef", 3);

        Assert.Equal("abcdef", source.ToString());
    }

    [Fact]
    public void FromString_CreatesSourceWithZeroOffset()
    {
        var source = Source.FromString("abcdef");

        Assert.Equal(0, source.Offset);
        Assert.Equal("abcdef", source.ToString());
    }

    [Fact]
    public void ReadSymbol_EndOfSource_ReturnsEof()
    {
        var source = new Source("abc");
        var symbol = new Symbol(3, 0, Pattern.EndOfSource);

        Assert.Equal("EOF", source.ReadSymbol(in symbol));
    }

    [Fact]
    public void ReadSymbol_NoMatch_ReturnsLexerError()
    {
        var source = new Source("abc");
        var symbol = new Symbol(2, 0, Pattern.NoMatch);

        Assert.Equal("lexer error at offset: 2", source.ReadSymbol(in symbol));
    }

    [Fact]
    public void ReadSymbol_Match_ReturnsMatchedSlice()
    {
        var source = new Source("abcdef");
        var symbol = new Symbol(1, 3, 1);

        Assert.Equal("bcd", source.ReadSymbol(in symbol));
    }
}
