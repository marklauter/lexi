using System.Runtime.CompilerServices;

namespace Lexi;

public sealed class Lexer(
    Pattern[] patterns)
{
    private readonly Pattern[] patterns = patterns
        ?? throw new ArgumentNullException(nameof(patterns));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MatchResult NextMatch(MatchResult matchResult) => NextMatch(matchResult.Source);

    public MatchResult NextMatch(Source source)
    {
        if (source.IsEndOfSource)
        {
            return new(source, new(source.Offset, 0, Pattern.EndOfSource));
        }

        var offset = GetNextOffset(source);

        // Dragon book says perform all match tests.
        // Then return best match based on length and pattern set order.
        // Longest match wins.
        // Ties to go to first pattern in the set.
        var bestMatch = new SymbolMatch(new Symbol(0, 0, Pattern.NoMatch), 0);
        var patterns = this.patterns.AsSpan();
        var length = patterns.Length;
        var text = (string)source;
        for (var i = 0; i < length; ++i)
        {
            var symbolMatch = new SymbolMatch(
                patterns[i].Match(text, offset),
                i);

            if (symbolMatch.Symbol.IsMatch && CompareSymbolMatch(symbolMatch, bestMatch) > 0)
            {
                bestMatch = symbolMatch;
            }
        }

        var symbol = bestMatch.Symbol;

        return symbol.IsMatch
            ? new(
                new(text, offset + symbol.Length),
                symbol)
            : new(
                new(text, offset),
                new(offset, 0, Pattern.LexError));
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref struct SymbolMatch(
        Symbol symbol,
        int index)
    {
        public readonly Symbol Symbol = symbol;
        public readonly int Index = index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompareSymbolMatch(
        SymbolMatch l,
        SymbolMatch r)
    {
        var lLenth = l.Symbol.Length;
        var rLength = r.Symbol.Length;
        var lIndex = l.Index;
        var rIndex = r.Index;

        // longer match wins. tie goes to lowest index.
        return
            lLenth > rLength
            ? 1
            : lLenth < rLength
                ? -1
                : lIndex < rIndex
                    ? 1
                    : lIndex > rIndex
                        ? -1
                        : 0;
    }

    // todo: replace the idea of auto-ignore whitespace with Ignore Pattern that gets added to VocabularyBuilder with Ignore() method.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetNewLineOffset(Source source)
    {
        var offset = source.Offset;
        var match = CommonPatterns.NewLine()
            .Match(source, offset);
        if (match.Success)
        {
            offset += match.Length;
        }

        return offset;
    }

    // todo: replace the idea of auto-ignore whitespace with Ignore Pattern that gets added to VocabularyBuilder with Ignore() method.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetWhitespaceOffset(Source source, int offset)
    {
        var match = CommonPatterns.Whitespace()
            .Match(source, offset);
        if (match.Success)
        {
            offset += match.Length;
        }

        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetNextOffset(Source source)
    {
        var offset = GetNewLineOffset(source);
        return GetWhitespaceOffset(source, offset);
    }
}
