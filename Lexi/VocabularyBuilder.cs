using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Lexi;

public class VocabularyBuilder
{
    private readonly List<Pattern> patterns = [];

    private readonly RegexOptions regexOptions;

    private VocabularyBuilder(RegexOptions regexOptions) => this.regexOptions = regexOptions;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create() => new(RegexOptions.None);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create(RegexOptions regexOptions) => new(regexOptions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Lexer Build() => new([.. patterns]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        string pattern,
        int tokenId)
    {
        patterns.Add(new(pattern, tokenId, regexOptions));

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        Regex regex,
        int tokenId)
    {
        patterns.Add(new(regex, tokenId));

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(Pattern[] patterns)
    {
        this.patterns.AddRange(patterns);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match<TTokenId>(
        string pattern,
        TTokenId tokenId)
        where TTokenId : Enum
    {
        patterns.Add(Pattern.New(pattern, tokenId, regexOptions));

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match<TTokenId>(
        Regex regex,
        TTokenId tokenId)
        where TTokenId : Enum
    {
        patterns.Add(Pattern.New(regex, tokenId));

        return this;
    }
}
