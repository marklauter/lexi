using System.Runtime.CompilerServices;

namespace Lexi;

/// <summary>
/// A lexer named Lexi.
/// </summary>
/// <param name="matchPatterns"><see cref="Pattern"/></param>
/// <param name="ignorePatterns"><see cref="Pattern"/></param>
public sealed class Lexer(
    Pattern[] matchPatterns,
    Pattern[] ignorePatterns)
{
    private readonly Pattern[] matchPatterns = matchPatterns
        ?? throw new ArgumentNullException(nameof(matchPatterns));

    private readonly Pattern[] ignorePatterns = ignorePatterns
        ?? throw new ArgumentNullException(nameof(ignorePatterns));

    /// <summary>
    /// Returns the first match from the source text, starting at offset zero.
    /// </summary>
    /// <param name="source"><see cref="string"/></param>
    /// <returns><see cref="MatchResult"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MatchResult NextMatch(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return NextMatch(new Source(source));
    }

    /// <summary>
    /// Returns the next match from the source of the previous match.
    /// </summary>
    /// <param name="matchResult"><see cref="MatchResult"/></param>
    /// <returns><see cref="MatchResult"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MatchResult NextMatch(MatchResult matchResult) => NextMatch(matchResult.Source);

    /// <summary>
    /// Returns the next match from the source.
    /// </summary>
    /// <param name="source"><see cref="Source"/></param>
    /// <returns><see cref="MatchResult"/></returns>
    public MatchResult NextMatch(Source source)
    {
        if (source.IsEndOfSource)
        {
            return new(source, new Symbol(source.Offset, 0, Pattern.EndOfSource));
        }

        var span = source.Span;
        var offset = NextOffset(span, source.Offset);

        // Skipping ignore patterns can consume the rest of the source (trailing whitespace, comments).
        // Report that as end-of-source, not a NoMatch — otherwise a streaming consumer sees a phantom error token.
        if (offset >= span.Length)
        {
            return new(new Source(span, offset), new Symbol(offset, 0, Pattern.EndOfSource));
        }

        // Dragon book: perform all match tests, take the best (longest; ties to lowest pattern index).
        var best = new Symbol(offset, 0, Pattern.NoMatch);
        var bestIndex = int.MaxValue;
        var patterns = matchPatterns;
        for (var i = 0; i < patterns.Length; ++i)
        {
            var candidate = patterns[i].Match(span, offset);
            if (!candidate.IsMatch)
            {
                continue;
            }

            if (!best.IsMatch
                || candidate.Length > best.Length
                || candidate.Length == best.Length && i < bestIndex)
            {
                best = candidate;
                bestIndex = i;
            }
        }

        // On failure, span the single offending character (offset < span.Length is guaranteed by the
        // end-of-source check above) so the caller can read *what* broke, not just where. The offset
        // itself does not advance — recovery is the caller's choice.
        return best.IsMatch
            ? new(new Source(span, offset + best.Length), best)
            : new(new Source(span, offset), new Symbol(offset, 1, Pattern.NoMatch));
    }

    private int NextOffset(ReadOnlySpan<char> span, int offset)
    {
        // Loop until a full pass advances nothing: a single pass can't skip interleaved runs of
        // different ignorables (e.g. whitespace then a comment then whitespace). Each match advances
        // by at least one char (Symbol.IsMatch requires Length > 0), so this always terminates.
        var patterns = ignorePatterns;
        bool advanced;
        do
        {
            advanced = false;
            foreach (var pattern in patterns)
            {
                var match = pattern.Match(span, offset);
                if (match.IsMatch)
                {
                    offset += match.Length;
                    advanced = true;
                }
            }
        }
        while (advanced);

        return offset;
    }
}
