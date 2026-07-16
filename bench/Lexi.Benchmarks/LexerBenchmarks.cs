using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Lexi.Benchmarks;

// ShortRunJob: fewer iterations for a fast spike read. Swap for the default job when a publishable number is wanted.

/// <summary>
/// A/Bs the shipping string-based lexer (baseline) against the span-first prototype, tokenizing the same
/// input with the same vocabulary to end of source. Counts tokens only — no text extraction — to isolate the
/// per-token match cost. The hypothesis: the string lexer's <c>regex.Match</c> allocates a Match object per
/// pattern per token; the span lexer's <c>EnumerateMatches</c> allocates nothing.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class LexerBenchmarks
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
    private global::Lexi.Lexer stringLexer = null!;
    private global::Lexi.Spans.Lexer spanLexer = null!;

    [Params(1_000, 10_000)]
    public int Repeats { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        source = string.Concat(Enumerable.Repeat("foo + 123 * bar - 45.6 / (baz = 7) ", Repeats));

        stringLexer = global::Lexi.VocabularyBuilder
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

        spanLexer = global::Lexi.Spans.VocabularyBuilder
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

        // Correctness gate: a throughput comparison is meaningless unless both lexers tokenize identically.
        var stringCount = StringLexer();
        var spanCount = SpanLexer();
        if (stringCount != spanCount)
        {
            throw new InvalidOperationException($"token count mismatch: string={stringCount}, span={spanCount}");
        }
    }

    [Benchmark(Baseline = true)]
    public int StringLexer()
    {
        var count = 0;
        var match = stringLexer.NextMatch(source);
        while (match.Symbol.IsMatch)
        {
            ++count;
            match = stringLexer.NextMatch(match);
        }

        return count;
    }

    [Benchmark]
    public int SpanLexer()
    {
        var count = 0;
        var src = new global::Lexi.Spans.Source(source.AsSpan());
        var offset = 0;
        while (true)
        {
            var symbol = spanLexer.NextMatch(src, offset);
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
