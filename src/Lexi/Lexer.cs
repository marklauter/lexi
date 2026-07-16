using System.Runtime.CompilerServices;

namespace Lexi;

/// <summary>
/// Scans source text into tokens using a set of match patterns while skipping a set of ignore patterns.
/// Each call to <see cref="NextMatch(Source)"/> returns one <see cref="MatchResult"/>; feed its
/// <see cref="MatchResult.Source"/> back in to read the next token.
/// </summary>
/// <param name="matchPatterns">The <see cref="Pattern"/> set the lexer matches into tokens.</param>
/// <param name="ignorePatterns">The <see cref="Pattern"/> set the lexer skips between tokens.</param>
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
    /// <remarks>
    /// There are three outcomes. At the end of the source, the symbol carries
    /// <see cref="Pattern.EndOfSource"/>. On a successful match, the symbol is the longest match, with
    /// ties broken toward the lowest pattern index. On failure, the symbol carries
    /// <see cref="Pattern.NoMatch"/> and spans the single offending character without advancing the offset.
    /// </remarks>
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
        // end-of-source check above) so the caller can read the character that broke the lex. The offset
        // itself does not advance; recovery is the caller's choice.
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
