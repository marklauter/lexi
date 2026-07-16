using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Lexi.Spans;

/// <summary>
/// Span-first pattern. Matches over a <see cref="ReadOnlySpan{Char}"/> via
/// <see cref="Regex.EnumerateMatches(ReadOnlySpan{char}, int)"/>, which allocates no <see cref="Match"/>
/// object — unlike the string lexer's <c>regex.Match(text, offset)</c>, which allocates one per call.
/// </summary>
public sealed class Pattern
{
    /// <summary>const end of source token id</summary>
    public const uint EndOfSource = 1U << 31;

    /// <summary>const lex error token id</summary>
    public const uint NoMatch = 1U << 30;

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
            ? throw new ArgumentOutOfRangeException(nameof(tokenId), $"Reserved token id conflict. Values over {NoMatch} are reserved.")
            : tokenId;
    }

    private Pattern(string pattern, uint tokenId, RegexOptions regexOptions)
        : this(new Regex(@$"\G(?:{pattern ?? throw new ArgumentNullException(nameof(pattern))})", DefaultRegexOptions | regexOptions), tokenId)
    { }

    /// <summary>Creates a new pattern with the given options.</summary>
    public static Pattern New(string pattern, uint tokenId, RegexOptions regexOptions) =>
        new(pattern, tokenId, regexOptions);

    /// <summary>Creates a new pattern with the default options.</summary>
    public static Pattern New(string pattern, uint tokenId) =>
        new(pattern, tokenId, DefaultRegexOptions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Symbol Match(ReadOnlySpan<char> source, int offset)
    {
        // \G-anchored at startat: the first yielded match is at offset, or there is none. No Match allocation.
        foreach (var match in regex.EnumerateMatches(source, offset))
        {
            return new Symbol(match.Index, match.Length, tokenId);
        }

        return new Symbol(offset, 0, tokenId | NoMatch);
    }
}
