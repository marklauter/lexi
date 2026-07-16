# Lexi benchmarks (spike)

A/B of the shipping string-based lexer (`Lexi`) against a span-first prototype (`Lexi.Spans`).
Both tokenize the same input with the same vocabulary to end of source, counting tokens only (no text
extraction) to isolate the per-token match cost. A correctness gate in `[GlobalSetup]` asserts both lexers
produce identical token counts before timing.

Kept out of `Lexi.slnx` so it never touches the PR gate/coverage. Lives on `spike/span-first-lexer`.

## Run

```
dotnet run -c Release --project bench/Lexi.Benchmarks
```

## The two changes under test

1. **`Pattern.Match` over `ReadOnlySpan<char>` via `Regex.EnumerateMatches`** instead of `regex.Match(text, offset)`.
   `Match` allocates a `System.Text.RegularExpressions.Match` object per call — i.e. per pattern, per token.
   `EnumerateMatches` yields `ValueMatch` structs and allocates nothing.
2. **`Symbol` is a plain `readonly record struct`, not a `ref struct`.** It carries only positions
   (offset/length/token id), never the span, so tokens are freely collectible/streamable. The span lives
   only in `Source` (a ref struct). Decoupling "where the token is" from "what the text is" is what removes
   the ref-struct wall that blocks `IEnumerable<Symbol>` / `List<Symbol>` / async.

## First results (ShortRunJob, net10.0, Release)

| Method      | Repeats | Mean     | Ratio | Allocated | Alloc Ratio |
|-------------|--------:|---------:|------:|----------:|------------:|
| StringLexer |   1,000 |  2.45 ms |  1.00 |    5.2 MB |        1.00 |
| SpanLexer   |   1,000 |  1.80 ms |  0.73 |       0 B |        0.00 |
| StringLexer |  10,000 | 25.19 ms |  1.00 |     52 MB |        1.00 |
| SpanLexer   |  10,000 | 18.12 ms |  0.72 |       0 B |        0.00 |

**Zero allocation** (the per-pattern-per-token `Match` object is gone) and **~27% faster**. The allocation
win is the headline; the speedup rides along on top.

> Numbers are a fast `[ShortRunJob]` read. Drop the `[ShortRunJob]` attribute for a publishable measurement.
