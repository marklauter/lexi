using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Lexi;

/// <summary>
/// Builds the match and ignore pattern sets for a <see cref="Lexer"/>.
/// </summary>
public sealed class VocabularyBuilder
{
    private readonly List<Pattern> matchPatterns = [];
    private readonly List<Pattern> ignorePatterns = [];

    private readonly RegexOptions regexOptions;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private VocabularyBuilder(RegexOptions regexOptions) => this.regexOptions = regexOptions;

    /// <summary>
    /// Creates a new <see cref="VocabularyBuilder"/> with <see cref="RegexOptions.None"/>.
    /// </summary>
    /// <returns>A new <see cref="VocabularyBuilder"/>.</returns>
    /// <seealso cref="RegexOptions"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create() => new(RegexOptions.None);

    /// <summary>
    /// Creates a new <see cref="VocabularyBuilder"/> with the specified <see cref="RegexOptions"/>.
    /// </summary>
    /// <param name="regexOptions">The default <see cref="RegexOptions"/> the builder uses to create <see cref="Regex"/> patterns from strings.</param>
    /// <returns>A new <see cref="VocabularyBuilder"/>.</returns>
    /// <remarks>
    /// RegexOptions may be overridden in calls to Match and Ignore.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VocabularyBuilder Create(RegexOptions regexOptions) => new(regexOptions);

    /// <summary>
    /// Creates a new <see cref="Lexer"/> from the match and ignore patterns collected by the builder.
    /// </summary>
    /// <returns>A new <see cref="Lexer"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Lexer Build() => new([.. matchPatterns], [.. ignorePatterns]);

    /// <summary>
    /// Adds a pattern to the match list.
    /// </summary>
    /// <param name="pattern">The regular expression string from which to build a <see cref="Regex"/> to add to the match list.</param>
    /// <param name="tokenId">The token identifier for the pattern. Must be less than <see cref="Pattern.NoMatch"/>.</param>
    /// <returns>The same <see cref="VocabularyBuilder"/> instance for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        string pattern,
        uint tokenId)
    {
        matchPatterns.Add(Pattern.New(pattern, tokenId, regexOptions));
        return this;
    }

    /// <summary>
    /// Adds a pattern to the match list.
    /// </summary>
    /// <param name="regex">The <see cref="Regex"/> to add to the match list.</param>
    /// <param name="tokenId">The token identifier for the pattern. Must be less than <see cref="Pattern.NoMatch"/>.</param>
    /// <returns>The same <see cref="VocabularyBuilder"/> instance for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(
        Regex regex,
        uint tokenId)
    {
        matchPatterns.Add(Pattern.New(regex, tokenId));
        return this;
    }

    /// <summary>
    /// Adds a set of patterns to the match list.
    /// </summary>
    /// <param name="patterns">A prebuilt <see cref="Pattern"/> set to append to the match list.</param>
    /// <returns>The same <see cref="VocabularyBuilder"/> instance for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Match(Pattern[] patterns)
    {
        matchPatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Adds a pattern to the ignore list.
    /// </summary>
    /// <param name="pattern">The regular expression string from which to build a <see cref="Regex"/> to add to the ignore list.</param>
    /// <param name="tokenId">The token identifier for the pattern. Must be less than <see cref="Pattern.NoMatch"/>.</param>
    /// <returns>The same <see cref="VocabularyBuilder"/> instance for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(
        string pattern,
        uint tokenId)
    {
        ignorePatterns.Add(Pattern.New(pattern, tokenId, regexOptions));
        return this;
    }

    /// <summary>
    /// Adds a pattern to the ignore list.
    /// </summary>
    /// <param name="regex">The <see cref="Regex"/> to add to the ignore list.</param>
    /// <param name="tokenId">The token identifier for the pattern. Must be less than <see cref="Pattern.NoMatch"/>.</param>
    /// <returns>The same <see cref="VocabularyBuilder"/> instance for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(
        Regex regex,
        uint tokenId)
    {
        ignorePatterns.Add(Pattern.New(regex, tokenId));
        return this;
    }

    /// <summary>
    /// Adds a set of patterns to the ignore list.
    /// </summary>
    /// <param name="patterns">A prebuilt <see cref="Pattern"/> set to append to the ignore list.</param>
    /// <returns>The same <see cref="VocabularyBuilder"/> instance for chaining.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VocabularyBuilder Ignore(Pattern[] patterns)
    {
        ignorePatterns.AddRange(patterns);
        return this;
    }
}
