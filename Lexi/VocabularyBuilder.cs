using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Lexi;

public class VocabularyBuilder
{
    private readonly List<Pattern> matchPatterns = [];
    private readonly List<Pattern> ignorePatterns = [];

    private readonly RegexOptions regexOptions;

    private VocabularyBuilder(RegexOptions regexOptions) => this.regexOptions = regexOptions;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create() => new(RegexOptions.None);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create(RegexOptions regexOptions) => new(regexOptions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Lexer Build() => new([.. matchPatterns], [.. ignorePatterns]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        string pattern,
        uint tokenId)
    {
        matchPatterns.Add(new(pattern, tokenId, regexOptions));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        Regex regex,
        uint tokenId)
    {
        matchPatterns.Add(new(regex, tokenId));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(Pattern[] patterns)
    {
        matchPatterns.AddRange(patterns);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(
        string pattern,
        uint tokenId)
    {
        ignorePatterns.Add(new(pattern, tokenId, regexOptions));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(
        Regex regex,
        uint tokenId)
    {
        ignorePatterns.Add(new(regex, tokenId));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(Pattern[] patterns)
    {
        ignorePatterns.AddRange(patterns);
        return this;
    }
}
