using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Lexi.Benchmarks;

/// <summary>
/// Experiment 2 — "What does full span-first buy, end-to-end?" (breaking). Measures the total real-world win
/// when a consumer also reads each token's text.
/// <list type="bullet">
/// <item>Baseline: the shipping <see cref="global::Lexi.Lexer"/>, tokenize + <c>Source.ReadSymbol</c> per
/// token (a substring allocation per token).</item>
/// <item>Experiment: <see cref="global::Lexi.Spans.Lexer"/>, tokenize + <c>Source.Slice</c> per token (a
/// span, no allocation).</item>
/// </list>
/// Both arms fold every character of every token into a rolling content hash — this consumes the extracted
/// text (so it is not optimized away) and, via the <c>[GlobalSetup]</c> gate, proves both arms extract
/// byte-for-byte identical text. Exp 2 delta = total win; exp 2 minus exp 1 ≈ the marginal value of the span
/// rewrite over the cheap swap, which is what the breaking v3 change has to justify.
/// </summary>
[MemoryDiagnoser]
public class Experiment2EndToEnd
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
    private global::Lexi.Spans.Lexer spanLexer = null!;

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

        // Correctness gate: both arms must extract byte-for-byte identical text, hence identical hashes.
        var shippingHash = ShippingReadSymbol();
        var spanHash = SpanSlice();
        if (shippingHash != spanHash)
        {
            throw new InvalidOperationException(
                $"extracted-text hash mismatch: shipping={shippingHash}, span={spanHash}");
        }
    }

    [Benchmark(Baseline = true)]
    public long ShippingReadSymbol()
    {
        long hash = 17;
        var match = shippingLexer.NextMatch(source);
        while (match.Symbol.IsMatch)
        {
            var text = match.Source.ReadSymbol(in match.Symbol); // substring allocation per token
            foreach (var c in text)
            {
                hash = (hash * 31) + c;
            }

            match = shippingLexer.NextMatch(match);
        }

        return hash;
    }

    [Benchmark]
    public long SpanSlice()
    {
        long hash = 17;
        var src = new global::Lexi.Spans.Source(source.AsSpan());
        var offset = 0;
        while (true)
        {
            var symbol = spanLexer.NextMatch(src, offset);
            if (!symbol.IsMatch)
            {
                break;
            }

            var text = src.Slice(symbol); // span, no allocation
            foreach (var c in text)
            {
                hash = (hash * 31) + c;
            }

            offset = symbol.Offset + symbol.Length;
        }

        return hash;
    }
}
