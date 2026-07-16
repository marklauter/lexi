using System.Text.RegularExpressions;

namespace Lexi.Spans;

/// <summary>Builds a span-first <see cref="Lexer"/> from match and ignore patterns.</summary>
public sealed class VocabularyBuilder
{
    private readonly List<Pattern> matchPatterns = [];
    private readonly List<Pattern> ignorePatterns = [];
    private readonly RegexOptions regexOptions;

    private VocabularyBuilder(RegexOptions regexOptions) => this.regexOptions = regexOptions;

    /// <summary>Creates a builder with <see cref="RegexOptions.None"/>.</summary>
    public static VocabularyBuilder Create() => new(RegexOptions.None);

    /// <summary>Creates a builder with the given options.</summary>
    public static VocabularyBuilder Create(RegexOptions regexOptions) => new(regexOptions);

    /// <summary>Adds a match pattern.</summary>
    public VocabularyBuilder Match(string pattern, uint tokenId)
    {
        matchPatterns.Add(Pattern.New(pattern, tokenId, regexOptions));
        return this;
    }

    /// <summary>Adds an ignore pattern.</summary>
    public VocabularyBuilder Ignore(string pattern, uint tokenId)
    {
        ignorePatterns.Add(Pattern.New(pattern, tokenId, regexOptions));
        return this;
    }

    /// <summary>Builds the lexer.</summary>
    public Lexer Build() => new([.. matchPatterns], [.. ignorePatterns]);
}
