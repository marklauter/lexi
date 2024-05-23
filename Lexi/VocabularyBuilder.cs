using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Lexi;

/// <summary>
/// Vocabulary builder let's users easily build a set of lexer patterns to either match or ignore.
/// </summary>
public class VocabularyBuilder
{
    private readonly List<Pattern> matchPatterns = [];
    private readonly List<Pattern> ignorePatterns = [];

    private readonly RegexOptions regexOptions;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private VocabularyBuilder(RegexOptions regexOptions) => this.regexOptions = regexOptions;

    /// <summary>
    /// Creates a new instance of <see cref="VocabularyBuilder"/> with RegexOptions.None. <seealso cref="RegexOptions"/>
    /// </summary>
    /// <returns><see cref="VocabularyBuilder"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create() => new(RegexOptions.None);

    /// <summary>
    /// Creates a new instance of <see cref="VocabularyBuilder"/> with the specified <see cref="RegexOptions"/>.
    /// </summary>
    /// <param name="regexOptions">The default <see cref="RegexOptions"/> used by the builder to create Regex patterns from strings.</param>
    /// <returns><see cref="VocabularyBuilder"/></returns>
    /// <remarks>
    /// RegexOptions may be overridden in calls to Match and Ignore.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create(RegexOptions regexOptions) => new(regexOptions);

    /// <summary>
    /// Creates a new instance of <see cref="Lexer"/> from the patterns built by the builder.
    /// </summary>
    /// <returns><see cref="Lexer"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Lexer Build() => new([.. matchPatterns], [.. ignorePatterns]);

    /// <summary>
    /// Match adds a pattern to the match list.
    /// </summary>
    /// <param name="pattern">The regular expression string from which to build a <see cref="Regex"/> to add to the match list.</param>
    /// <param name="tokenId">The token identifier for the pattern.</param>
    /// <returns><see cref="VocabularyBuilder"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        string pattern,
        uint tokenId)
    {
        matchPatterns.Add(Pattern.New(pattern, tokenId, regexOptions));
        return this;
    }

    /// <summary>
    /// Match adds a pattern to the match list.
    /// </summary>
    /// <param name="regex">The <see cref="Regex"/> to add to the match list.</param>
    /// <param name="tokenId">The token identifier for the pattern.</param>
    /// <returns><see cref="VocabularyBuilder"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        Regex regex,
        uint tokenId)
    {
        matchPatterns.Add(Pattern.New(regex, tokenId));
        return this;
    }

    /// <summary>
    /// Match adds a set of patterns to the match list.
    /// </summary>
    /// <param name="patterns">A prebuilt <see cref="Pattern"/> set to be appened to the match list.</param>
    /// <returns><see cref="VocabularyBuilder"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(Pattern[] patterns)
    {
        matchPatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Ignore adds a pattern to the ignore list.
    /// </summary>
    /// <param name="pattern">The regular expression string from which to build a <see cref="Regex"/> to add to the ignore list.</param>
    /// <param name="tokenId">The token identifier for the pattern.</param>
    /// <returns><see cref="VocabularyBuilder"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(
        string pattern,
        uint tokenId)
    {
        ignorePatterns.Add(Pattern.New(pattern, tokenId, regexOptions));
        return this;
    }

    /// <summary>
    /// Ignore adds a pattern to the ignore list.
    /// </summary>
    /// <param name="regex">The <see cref="Regex"/> to add to the ignore list.</param>
    /// <param name="tokenId">The token identifier for the pattern.</param>
    /// <returns><see cref="VocabularyBuilder"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(
        Regex regex,
        uint tokenId)
    {
        ignorePatterns.Add(Pattern.New(regex, tokenId));
        return this;
    }

    /// <summary>
    /// Ignore adds a set of patterns to the ignore list.
    /// </summary>
    /// <param name="patterns">A prebuilt <see cref="Pattern"/> set to be appened to the ignore list.</param>
    /// <returns><see cref="VocabularyBuilder"/></returns>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(Pattern[] patterns)
    {
        ignorePatterns.AddRange(patterns);
        return this;
    }
}
