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
            return new(source, new(source.Offset, 0, Pattern.EndOfSource));
        }

        var offset = NextOffset(source);

        // Dragon book says perform all match tests.
        // Then return best match based on length and pattern set index.
        var text = (string)source;
        var bestMatch = new SymbolMatch(new(offset, 0, Pattern.NoMatch), Int32.MaxValue);
        var patterns = matchPatterns;
        var length = patterns.Length;
        for (var i = 0; i < length; ++i)
        {
            bestMatch = CompareAndSwap(patterns[i].Match(text, offset), i, bestMatch);
        }

        var symbol = bestMatch.Symbol;

        return symbol.IsMatch
            ? new(new(text, offset + symbol.Length), symbol)
            : new(new(text, offset), symbol);
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref struct SymbolMatch(
        Symbol symbol,
        int index)
    {
        public readonly Symbol Symbol = symbol;
        public readonly int Index = index;
    }

    private int NextOffset(Source source)
    {
        var offset = source.Offset;

        var patterns = ignorePatterns;
        foreach (var pattern in patterns)
        {
            var match = pattern.Match(source, offset);
            if (match.IsMatch)
            {
                offset += match.Length;
            }
        }

        return offset;
    }

    // no match is not swap candidate
    // longer match wins
    // tie goes to lowest index
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SymbolMatch CompareAndSwap(Symbol nextSymbol, int index, SymbolMatch bestMatch) =>
        !nextSymbol.IsMatch
            ? bestMatch
            : nextSymbol.Length.CompareTo(bestMatch.Symbol.Length) switch
            {
                > 0 => new(nextSymbol, index),
                < 0 => bestMatch,
                0 or _ => index < bestMatch.Index ? new(nextSymbol, index) : bestMatch,
            };
}
