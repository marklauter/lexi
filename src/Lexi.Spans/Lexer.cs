using System.Runtime.CompilerServices;

namespace Lexi.Spans;

/// <summary>
/// Span-first lexer. <see cref="NextMatch"/> returns a bare <see cref="Symbol"/> — the caller advances the
/// offset by <see cref="Symbol.Length"/>. Because the returned token carries no span, the caller may collect
/// the tokens into a list, yield them, or carry them across an await.
/// </summary>
public sealed class Lexer(
    Pattern[] matchPatterns,
    Pattern[] ignorePatterns)
{
    private readonly Pattern[] matchPatterns = matchPatterns
        ?? throw new ArgumentNullException(nameof(matchPatterns));

    private readonly Pattern[] ignorePatterns = ignorePatterns
        ?? throw new ArgumentNullException(nameof(ignorePatterns));

    /// <summary>Returns the token at or after <paramref name="offset"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Symbol NextMatch(Source source, int offset)
    {
        var span = source.Span;
        if ((uint)offset >= (uint)span.Length)
        {
            return new Symbol(Math.Min(offset, span.Length), 0, Pattern.EndOfSource);
        }

        var start = SkipIgnored(span, offset);
        if (start >= span.Length)
        {
            return new Symbol(start, 0, Pattern.EndOfSource);
        }

        // Dragon book: perform all match tests, take the best (longest; ties to lowest pattern index).
        var best = new Symbol(start, 0, Pattern.NoMatch);
        var bestIndex = int.MaxValue;
        var patterns = matchPatterns;
        for (var i = 0; i < patterns.Length; ++i)
        {
            var candidate = patterns[i].Match(span, start);
            if (!candidate.IsMatch)
            {
                continue;
            }

            if (!best.IsMatch
                || candidate.Length > best.Length
                || (candidate.Length == best.Length && i < bestIndex))
            {
                best = candidate;
                bestIndex = i;
            }
        }

        return best;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int SkipIgnored(ReadOnlySpan<char> span, int offset)
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
