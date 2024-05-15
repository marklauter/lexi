using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Lexi;

[DebuggerDisplay("{id}, {regex}")]
public sealed class Pattern(
    Regex regex,
    int tokenId)
{
    public static Pattern New<TId>(string pattern, TId tokenId) where TId : Enum =>
        new(pattern, Convert.ToInt32(tokenId, CultureInfo.InvariantCulture));

    public static Pattern New<TId>(string pattern, TId tokenId, RegexOptions regexOptions) where TId : Enum =>
        new(pattern, Convert.ToInt32(tokenId, CultureInfo.InvariantCulture), regexOptions);

    public static Pattern New<TId>(Regex regex, TId tokenId) where TId : Enum =>
        new(regex, Convert.ToInt32(tokenId, CultureInfo.InvariantCulture));

    internal const RegexOptions DefaultRegexOptions =
        RegexOptions.ExplicitCapture |
        RegexOptions.Compiled |
        RegexOptions.Singleline;

    private readonly Regex regex = regex ?? throw new ArgumentNullException(nameof(regex));
    private readonly int tokenId = tokenId is EndOfSource or LexError or NoMatch
        ? throw new ArgumentOutOfRangeException($"TokenId can't match any of the reserved values; {EndOfSource}, {LexError}, nor {NoMatch}.")
        : tokenId;

    public Pattern(
        string pattern,
        int tokenId,
        RegexOptions regexOptions)
        : this(
              new Regex(
                  @$"\G(?:{pattern})" ?? throw new ArgumentNullException(nameof(pattern)),
                  DefaultRegexOptions | regexOptions),
              tokenId)
    { }

    public Pattern(
        string pattern,
        int tokenId)
    : this(pattern, tokenId, DefaultRegexOptions)
    { }

    public const int EndOfSource = -1000;
    public const int LexError = -2000;
    public const int NoMatch = -3000;

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
