using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Lexi;

/// <summary>
/// A pattern used by the lexer to find tokens in the source.
/// </summary>
/// <remarks>
/// Use <see cref="VocabularyBuilder"/> to build the lexer's vocabulary.
/// </remarks>
[DebuggerDisplay("{tokenId}, {regex}")]
public sealed class Pattern
{
    /// <summary>
    /// Reserved token id marking the end of the source. Equal to <c>1U &lt;&lt; 31</c>.
    /// </summary>
    public const uint EndOfSource = 1U << 31;

    /// <summary>
    /// Reserved token id marking a lexer error where no pattern matched. Equal to <c>1U &lt;&lt; 30</c>.
    /// </summary>
    public const uint NoMatch = 1U << 30;

    /// <summary>
    /// Creates a new pattern.
    /// </summary>
    /// <param name="pattern">The regular expression to be converted into a <see cref="Regex"/>.</param>
    /// <param name="tokenId">The token identifier for the pattern. Must be less than <see cref="NoMatch"/>.</param>
    /// <returns>The new <see cref="Pattern"/>.</returns>
    public static Pattern New(string pattern, uint tokenId) =>
        new(pattern, tokenId);

    /// <summary>
    /// Creates a new pattern.
    /// </summary>
    /// <param name="pattern">The regular expression to be converted into a <see cref="Regex"/>.</param>
    /// <param name="tokenId">The token identifier for the pattern. Must be less than <see cref="NoMatch"/>.</param>
    /// <param name="regexOptions">The <see cref="RegexOptions"/> applied when compiling the pattern.</param>
    /// <returns>The new <see cref="Pattern"/>.</returns>
    public static Pattern New(string pattern, uint tokenId, RegexOptions regexOptions) =>
        new(pattern, tokenId, regexOptions);

    /// <summary>
    /// Creates a new pattern.
    /// </summary>
    /// <param name="regex">The <see cref="Regex"/> used to match the token.</param>
    /// <param name="tokenId">The token identifier for the pattern. Must be less than <see cref="NoMatch"/>.</param>
    /// <returns>The new <see cref="Pattern"/>.</returns>
    public static Pattern New(Regex regex, uint tokenId) =>
        new(regex, tokenId);

    internal const RegexOptions DefaultRegexOptions =
        RegexOptions.ExplicitCapture |
        RegexOptions.Compiled |
        RegexOptions.Singleline;

    private readonly Regex regex;
    private readonly uint tokenId;

    private Pattern(Regex regex, uint tokenId)
    {
        this.regex = regex ?? throw new ArgumentNullException(nameof(regex));
        this.tokenId = tokenId >= NoMatch
            ? throw new ArgumentOutOfRangeException($"Reserved token id conflict. Values over {NoMatch} are reserved.")
            : tokenId;
    }

    private Pattern(string pattern, uint tokenId, RegexOptions regexOptions)
        : this(new Regex(@$"\G(?:{pattern ?? throw new ArgumentNullException(nameof(pattern))})", DefaultRegexOptions | regexOptions), tokenId)
    { }

    private Pattern(string pattern, uint tokenId)
        : this(pattern, tokenId, DefaultRegexOptions)
    { }

    internal Symbol Match(
        ReadOnlySpan<char> source,
        int offset)
    {
        // \G-anchored at startat: the first yielded match is at offset, or there is none. EnumerateMatches
        // yields ValueMatch structs and allocates no Match object — unlike regex.Match(source, offset).
        foreach (var match in regex.EnumerateMatches(source, offset))
        {
            return new(match.Index, match.Length, tokenId);
        }

        return new(offset, 0, tokenId | Pattern.NoMatch);
    }
}
