using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Lexi;

[DebuggerDisplay("{tokenId}, {regex}")]
public sealed class Pattern
{
    public const uint EndOfSource = UInt32.MaxValue;
    public const uint LexError = UInt32.MaxValue - 1;

    public static Pattern New(string pattern, uint tokenId) =>
        new(pattern, tokenId);

    public static Pattern New(string pattern, uint tokenId, RegexOptions regexOptions) =>
        new(pattern, tokenId, regexOptions);

    public static Pattern New(Regex regex, uint tokenId) =>
        new(regex, tokenId);

    internal const RegexOptions DefaultRegexOptions =
        RegexOptions.ExplicitCapture |
        RegexOptions.Compiled |
        RegexOptions.Singleline;

    private readonly Regex regex;
    private readonly uint tokenId;

    internal Pattern(Regex regex, uint tokenId)
    {
        this.regex = regex ?? throw new ArgumentNullException(nameof(regex));
        this.tokenId = tokenId is EndOfSource or LexError
            ? throw new ArgumentOutOfRangeException($"Reserved token id conflict. TokenId can't match: {EndOfSource}, {LexError}")
            : tokenId;
    }

    internal Pattern(string pattern, uint tokenId, RegexOptions regexOptions)
        : this(new Regex(@$"\G(?:{pattern})" ?? throw new ArgumentNullException(nameof(pattern)), DefaultRegexOptions | regexOptions), tokenId)
    { }

    internal Pattern(string pattern, uint tokenId)
        : this(pattern, tokenId, DefaultRegexOptions)
    { }

    internal Symbol Match(
        string source,
        int offset)
    {
        var match = regex.Match(source, offset);
        return match.Success
           ? new(match.Index, match.Length, tokenId)
           : new(offset, 0, tokenId);
    }
}
