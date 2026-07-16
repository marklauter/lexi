using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Lexi.Benchmarks;

/// <summary>
/// Experiment 1 — "Is the cheap swap enough?" (non-breaking). Isolates the regex-API swap from the
/// span-first type rewrite. Both arms are string-in and count tokens only (no text extraction).
/// <list type="bullet">
/// <item>Baseline: the shipping <see cref="global::Lexi.Lexer"/> (uses <c>regex.Match</c>).</item>
/// <item>Experiment: <see cref="CheapSwapLexer"/> — identical algorithm, but <c>regex.EnumerateMatches</c>
/// over the source span. No public span types, no breaking change.</item>
/// </list>
/// If the experiment hits ~0 alloc and most of the speedup, the win is available without the v3 break.
/// </summary>
[MemoryDiagnoser]
public class Experiment1CheapSwap
{
    private const uint Float = 1;
    private const uint Int = 2;
    private const uint Ident = 3;
    private const uint Op = 4;
    private const uint Open = 5;
    private const uint Close = 6;
    private const uint Eq = 7;
    private const uint Ws = 99;

    private string source = "";
    private global::Lexi.Lexer shippingLexer = null!;
    private CheapSwapLexer cheapSwapLexer = null!;

    [Params(1_000, 10_000)]
    public int Repeats { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        source = string.Concat(Enumerable.Repeat("foo + 123 * bar - 45.6 / (baz = 7) ", Repeats));

        shippingLexer = global::Lexi.VocabularyBuilder
            .Create(RegexOptions.CultureInvariant)
            .Match(@"\d+\.\d+", Float)
            .Match(@"\d+", Int)
            .Match(@"[a-zA-Z_]\w*", Ident)
            .Match(@"[+\-*/]", Op)
            .Match(@"\(", Open)
            .Match(@"\)", Close)
            .Match(@"=", Eq)
            .Ignore(@"\s+", Ws)
            .Build();

        cheapSwapLexer = new CheapSwapLexer(
            [
                CheapSwapPattern.New(@"\d+\.\d+", Float, RegexOptions.CultureInvariant),
                CheapSwapPattern.New(@"\d+", Int, RegexOptions.CultureInvariant),
                CheapSwapPattern.New(@"[a-zA-Z_]\w*", Ident, RegexOptions.CultureInvariant),
                CheapSwapPattern.New(@"[+\-*/]", Op, RegexOptions.CultureInvariant),
                CheapSwapPattern.New(@"\(", Open, RegexOptions.CultureInvariant),
                CheapSwapPattern.New(@"\)", Close, RegexOptions.CultureInvariant),
                CheapSwapPattern.New(@"=", Eq, RegexOptions.CultureInvariant),
            ],
            [CheapSwapPattern.New(@"\s+", Ws, RegexOptions.CultureInvariant)]);

        // Correctness gate: a throughput comparison is meaningless unless both arms tokenize identically.
        var shippingCount = Shipping();
        var cheapSwapCount = CheapSwap();
        if (shippingCount != cheapSwapCount)
        {
            throw new InvalidOperationException(
                $"token count mismatch: shipping={shippingCount}, cheapSwap={cheapSwapCount}");
        }
    }

    [Benchmark(Baseline = true)]
    public int Shipping()
    {
        var count = 0;
        var match = shippingLexer.NextMatch(source);
        while (match.Symbol.IsMatch)
        {
            ++count;
            match = shippingLexer.NextMatch(match);
        }

        return count;
    }

    [Benchmark]
    public int CheapSwap()
    {
        var count = 0;
        var offset = 0;
        while (true)
        {
            var symbol = cheapSwapLexer.NextMatch(source, offset);
            if (!symbol.IsMatch)
            {
                break;
            }

            ++count;
            offset = symbol.Offset + symbol.Length;
        }

        return count;
    }
}
