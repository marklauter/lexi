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

        return best.IsMatch
            ? new(new Source(span, offset + best.Length), best)
            : new(new Source(span, offset), best);
    }

    private int NextOffset(ReadOnlySpan<char> span, int offset)
    {
        var patterns = ignorePatterns;
        foreach (var pattern in patterns)
        {
            var match = pattern.Match(span, offset);
            if (match.IsMatch)
            {
                offset += match.Length;
            }
        }

        return offset;
    }
}
