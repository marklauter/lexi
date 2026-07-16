using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Lexi.Tests;

[ExcludeFromCodeCoverage]
public sealed class VocabularyBuilderTests
{
    private const uint Word = 1;
    private const uint Number = 2;
    private const uint Space = 3;

    [Fact]
    public void Create_NoArg_BuildsUsableLexer()
    {
        var lexer = VocabularyBuilder
            .Create()
            .Match(@"[a-z]+", Word)
            .Build();

        var match = lexer.NextMatch("abc");

        Assert.Equal(Word, match.Symbol.TokenId);
        Assert.Equal("abc", match.Source.ReadSymbol(in match.Symbol));
    }

    [Fact]
    public void Match_RegexOverload_AddsMatchPattern()
    {
        var lexer = VocabularyBuilder
            .Create()
            .Match(new Regex(@"\G[a-z]+"), Word)
            .Build();

        Assert.Equal(Word, lexer.NextMatch("abc").Symbol.TokenId);
    }

    [Fact]
    public void Match_PatternArray_AddsAllPatterns()
    {
        Pattern[] patterns =
        [
            Pattern.New(@"[a-z]+", Word),
            Pattern.New(@"\d+", Number),
        ];

        var lexer = VocabularyBuilder
            .Create()
            .Match(patterns)
            .Build();

        Assert.Equal(Word, lexer.NextMatch("abc").Symbol.TokenId);
        Assert.Equal(Number, lexer.NextMatch("123").Symbol.TokenId);
    }

    [Fact]
    public void Ignore_StringOverload_SkipsMatchedPrefix()
    {
        var lexer = VocabularyBuilder
            .Create()
            .Match(@"[a-z]+", Word)
            .Ignore(@"\s+", Space)
            .Build();

        var match = lexer.NextMatch("   abc");

        Assert.Equal(Word, match.Symbol.TokenId);
        Assert.Equal("abc", match.Source.ReadSymbol(in match.Symbol));
    }

    [Fact]
    public void Ignore_RegexOverload_SkipsMatchedPrefix()
    {
        var lexer = VocabularyBuilder
            .Create()
            .Match(@"[a-z]+", Word)
            .Ignore(new Regex(@"\G\s+"), Space)
            .Build();

        Assert.Equal(Word, lexer.NextMatch("  abc").Symbol.TokenId);
    }

    [Fact]
    public void Ignore_PatternArray_SkipsAllMatchedPrefixes()
    {
        Pattern[] ignore = [Pattern.New(@"\s+", Space)];

        var lexer = VocabularyBuilder
            .Create()
            .Match(@"[a-z]+", Word)
            .Ignore(ignore)
            .Build();

        Assert.Equal(Word, lexer.NextMatch(" abc").Symbol.TokenId);
    }

    [Fact]
    public void Create_WithRegexOptions_AppliesToStringPatterns()
    {
        var lexer = VocabularyBuilder
            .Create(RegexOptions.IgnoreCase)
            .Match(@"[a-z]+", Word)
            .Build();

        var match = lexer.NextMatch("ABC");

        Assert.Equal(Word, match.Symbol.TokenId);
        Assert.Equal("ABC", match.Source.ReadSymbol(in match.Symbol));
    }
}
