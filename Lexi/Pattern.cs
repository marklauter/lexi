using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Lexi;

[DebuggerDisplay("{id}, {regex}")]
public sealed class Pattern(
    Regex regex,
    int tokenId)
{
    internal const RegexOptions DefaultRegexOptions =
        RegexOptions.ExplicitCapture |
        RegexOptions.Compiled |
        RegexOptions.Singleline;

    private readonly int tokenId = tokenId;
    private readonly Regex regex = regex ?? throw new ArgumentNullException(nameof(regex));

    public Pattern(
        string pattern,
        RegexOptions regexOptions,
        int tokenId)
        : this(
              new Regex(
                  @$"\G(?:{pattern})" ?? throw new ArgumentNullException(nameof(pattern)),
                  DefaultRegexOptions | regexOptions),
              tokenId)
    { }

    public Pattern(
        string pattern,
        int tokenId)
    : this(
          pattern,
          DefaultRegexOptions,
          tokenId)
    { }

    // todo: try removing these to see if it breaks anything
    public const int EndOfSource = -1;
    public const int LexError = -2;
    public const int NoMatch = -3;

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
