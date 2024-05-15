using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Lexi;

public class VocabularyBuilder
{
    private readonly List<Pattern> patterns = [];

    private readonly RegexOptions regexOptions;

    private VocabularyBuilder(RegexOptions regexOptions)
    {
        this.regexOptions = regexOptions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create()
    {
        return new VocabularyBuilder(RegexOptions.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create(RegexOptions regexOptions)
    {
        return new VocabularyBuilder(regexOptions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Lexer Build()
    {
        return new Lexer([.. patterns]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        string pattern,
        int tokenId)
    {
        patterns.Add(new(pattern, regexOptions, tokenId));

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        Regex regex,
        int id)
    {
        patterns.Add(new(regex, id));

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(Pattern[] patterns)
    {
        this.patterns.AddRange(patterns);

        return this;
    }
}
