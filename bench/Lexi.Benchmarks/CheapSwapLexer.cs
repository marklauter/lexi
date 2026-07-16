using System.Text.RegularExpressions;

namespace Lexi.Benchmarks;

// A string-in lexer that is algorithmically identical to the shipping Lexi.Lexer — all patterns tested,
// longest match wins, ties break to the lowest pattern index, ignore patterns skipped once each — and
// differs in exactly ONE place: CheapSwapPattern.Match calls regex.EnumerateMatches(text.AsSpan(), offset)
// (which yields ValueMatch structs and allocates no Match object) instead of regex.Match(text, offset)
// (which allocates one Match object per pattern per token). No span-first public types: string in, token
// count out. This isolates the non-breaking regex-API swap from the breaking span-first type rewrite.

internal sealed class CheapSwapPattern
{
    public const uint EndOfSource = 1U << 31;
    public const uint NoMatch = 1U << 30;

    private const RegexOptions DefaultRegexOptions =
        RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline;

    private readonly Regex regex;
    private readonly uint tokenId;

    private CheapSwapPattern(string pattern, uint tokenId, RegexOptions regexOptions)
    {
        regex = new Regex(@$"\G(?:{pattern})", DefaultRegexOptions | regexOptions);
        this.tokenId = tokenId;
    }

    public static CheapSwapPattern New(string pattern, uint tokenId, RegexOptions regexOptions) =>
        new(pattern, tokenId, regexOptions);

    // The only line that differs from Lexi.Pattern.Match: EnumerateMatches over the span, no Match alloc.
    public CheapSwapSymbol Match(string source, int offset)
    {
        foreach (var match in regex.EnumerateMatches(source.AsSpan(), offset))
        {
            return new CheapSwapSymbol(match.Index, match.Length, tokenId);
        }

        return new CheapSwapSymbol(offset, 0, tokenId | NoMatch);
    }
}

internal readonly record struct CheapSwapSymbol(int Offset, int Length, uint TokenId)
{
    public bool IsMatch => (TokenId & CheapSwapPattern.NoMatch) == 0 && Length > 0 && !IsEndOfSource;

    public bool IsEndOfSource => (TokenId & CheapSwapPattern.EndOfSource) != 0;
}

internal sealed class CheapSwapLexer(CheapSwapPattern[] matchPatterns, CheapSwapPattern[] ignorePatterns)
{
    private readonly CheapSwapPattern[] matchPatterns = matchPatterns;
    private readonly CheapSwapPattern[] ignorePatterns = ignorePatterns;

    // Mirrors Lexi.Lexer.NextMatch: string source + explicit offset, so no Source/Symbol ref-struct wall
    // is involved. Advancing the offset by Length is the caller's job (see the benchmark loop).
    public CheapSwapSymbol NextMatch(string source, int offset)
    {
        if (offset >= source.Length)
        {
            return new CheapSwapSymbol(Math.Min(offset, source.Length), 0, CheapSwapPattern.EndOfSource);
        }

        var start = offset;
        foreach (var pattern in ignorePatterns)
        {
            var match = pattern.Match(source, start);
            if (match.IsMatch)
            {
                start += match.Length;
            }
        }

        if (start >= source.Length)
        {
            return new CheapSwapSymbol(start, 0, CheapSwapPattern.EndOfSource);
        }

        var best = new CheapSwapSymbol(start, 0, CheapSwapPattern.NoMatch);
        var bestIndex = int.MaxValue;
        for (var i = 0; i < matchPatterns.Length; ++i)
        {
            var candidate = matchPatterns[i].Match(source, start);
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
}
