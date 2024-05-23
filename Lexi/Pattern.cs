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
    /// const end of source token id
    /// </summary>
    public const uint EndOfSource = 1U << 31;

    /// <summary>
    /// const lex error token id
    /// </summary>
    public const uint NoMatch = 1U << 30;

    /// <summary>
    /// Creates a new pattern.
    /// </summary>
    /// <param name="pattern">The regular expression to be converted into a <see cref="Regex"/></param>
    /// <param name="tokenId">The token identifier for the pattern.</param>
    /// <returns></returns>
    public static Pattern New(string pattern, uint tokenId) =>
        new(pattern, tokenId);

    /// <summary>
    /// Creates a new pattern.
    /// </summary>
    /// <param name="pattern">The regular expression to be converted into a <see cref="Regex"/></param>
    /// <param name="tokenId">The token identifier for the pattern.</param>
    /// <param name="regexOptions"><see cref="RegexOptions"/></param>
    /// <returns></returns>
    public static Pattern New(string pattern, uint tokenId, RegexOptions regexOptions) =>
        new(pattern, tokenId, regexOptions);

    /// <summary>
    /// Creates a new pattern.
    /// </summary>
    /// <param name="regex"><see cref="Regex"/></param>
    /// <param name="tokenId">The token identifier for the pattern.</param>
    /// <returns></returns>
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
        : this(new Regex(@$"\G(?:{pattern})" ?? throw new ArgumentNullException(nameof(pattern)), DefaultRegexOptions | regexOptions), tokenId)
    { }

    private Pattern(string pattern, uint tokenId)
        : this(pattern, tokenId, DefaultRegexOptions)
    { }

    internal Symbol Match(
        string source,
        int offset)
    {
        var match = regex.Match(source, offset);
        return match.Success
           ? new(match.Index, match.Length, tokenId)
           : new(offset, 0, tokenId | Pattern.NoMatch);
    }
}
