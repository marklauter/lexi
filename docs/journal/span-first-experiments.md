# Span-first lexer — experiment log

Chronological, append-only. Each experiment records the control, the change, the numbers, and the takeaway,
so the effect of each is visible in sequence. Branch: `spike/span-first-lexer` (off `main`, not in `Lexi.slnx`).

## Background

The `spike/span-first-lexer` branch holds a parallel prototype `src/Lexi.Spans` (distinct namespace, identical
type names) and a BenchmarkDotNet A/B in `bench/Lexi.Benchmarks`. The prototype changes **two** things at once
vs the shipping string lexer:

1. **Regex API** — `regex.EnumerateMatches(span, offset)` instead of `regex.Match(text, offset)`. `Match`
   allocates a `System.Text.RegularExpressions.Match` object per call (i.e. per pattern, per token);
   `EnumerateMatches` yields `ValueMatch` structs and allocates nothing. *Non-breaking* if applied alone.
2. **Span-first types** — `Source` is a `ref struct` over `ReadOnlySpan<char>`; `Symbol` is a plain
   `readonly record struct` (no span → freely collectible); text is extracted via `Slice` (a span) instead of
   `ReadSymbol` (a substring, which allocates). *Breaking* (removes/changes the public string surface).

Initial `ShortRunJob` A/B (tokenize-only, count tokens): span lexer allocated **0 B** vs **5.2 MB / 52 MB**
(1k / 10k repeats) and ran **~27% faster**. But that conflated the two changes above, and measured only
`NextMatch` (no text extraction). The two experiments below de-conflate and make it realistic.

## Methodology (both experiments)

- One `[MemoryDiagnoser]` BenchmarkDotNet class per experiment. Shipping `Lexi.Lexer` is
  `[Benchmark(Baseline = true)]`; the new code is `[Benchmark]`. Same vocabulary, same input.
- `[Params]` sweeps input size (e.g. 1_000 and 10_000 repeats of a representative expression).
- `[GlobalSetup]` runs a **correctness gate**: both arms must produce identical output (token counts; for
  exp 2, identical extracted text). Throw on mismatch so the ratio is honest.
- **Full job** — no `[ShortRunJob]` — for publishable Mean / Ratio / Alloc Ratio.
- Only the filtered summary table is read back (`Method | Params | Mean | Ratio | Allocated | Alloc Ratio`),
  never the raw BDN log.
- Keep `bench/` and `src/Lexi.Spans` out of `Lexi.slnx`; confirm the main solution stays green; commit each
  experiment (result note + code) and push to `spike/span-first-lexer`.

## Experiment 1 — "Is the cheap swap enough?" (non-breaking)

**Question:** how much of the allocation/speed win comes from the regex-API swap alone, with no span-first
rewrite and no breaking change?

- **Control:** shipping `Lexi.Lexer` (uses `regex.Match`). Tokenize to end, count tokens.
- **Experiment:** a string-based lexer *identical in algorithm* to shipping Lexi (all patterns, longest match,
  ties to lowest index, ignore-skip) but with `Pattern.Match` doing `regex.EnumerateMatches(text.AsSpan(),
  offset)` and taking the first `ValueMatch`. No public span types; `string` in, same token-count out.
  Implement as a small type in the bench project (or a `Lexi.Spans` sibling) — do **not** edit shipping Lexi.
- **Measure:** tokenize-only (count). `MemoryDiagnoser`.
- **Read:** if the experiment also hits ~0 alloc and most of the speedup, the allocation win is available
  *without* the breaking rewrite — a cheap `Match → EnumerateMatches` change to the current lexer.

### Result 1

Full job, `[MemoryDiagnoser]`, net10.0, Release. Correctness gate passed (identical token counts). Baseline is
the shipping `Lexi.Lexer`; `CheapSwap` is the string-in lexer with the sole change being `Match →
EnumerateMatches`.

| Method    | Repeats | Mean      | Ratio | Allocated  | Alloc Ratio |
|---------- |-------- |----------:|------:|-----------:|------------:|
| Shipping  |   1,000 |  2.389 ms |  1.00 |  5,200,000 B |      1.00 |
| CheapSwap |   1,000 |  2.083 ms |  0.87 |          0 B |      0.00 |
| Shipping  |  10,000 | 24.004 ms |  1.00 | 52,000,000 B |      1.00 |
| CheapSwap |  10,000 | 21.168 ms |  0.88 |          0 B |      0.00 |

**Takeaway:** the entire allocation win is available from the non-breaking swap alone. `EnumerateMatches`
drops per-token allocation to **0 B** (from 5.2 MB / 52 MB — the per-pattern-per-token `Match` object, gone)
with **zero** change to the public surface: same `string` in, same token count out, same algorithm. The
speedup is a real but secondary **~12–13%** (ratio 0.87–0.88). Note this is smaller than the conflated
`ShortRunJob` read's ~27% — that number rode on span-first types *plus* short-run noise; isolated and
full-job, the regex swap by itself is a modest time win on top of a total allocation win. The allocation
elimination — not the speedup — is the headline, and it costs nothing.

## Experiment 2 — "What does full span-first buy, end-to-end?" (breaking)

**Question:** what is the total real-world win when a consumer also reads each token's text?

- **Control:** shipping `Lexi.Lexer`, tokenize + `Source.ReadSymbol(in symbol)` for each token (allocates a
  substring per token).
- **Experiment:** `Lexi.Spans.Lexer`, tokenize + `Source.Slice(symbol)` for each token (returns a span, no
  alloc). Consume the result so it isn't optimized away (e.g. sum lengths / xor first chars).
- **Measure:** tokenize + read. `MemoryDiagnoser`.
- **Read:** exp 2 delta = total win; exp 2 minus exp 1 ≈ the marginal value of the span rewrite over the cheap
  swap. That gap is what the breaking v3 change has to justify.

### Result 2
_(to be filled: paste the BDN summary table + one-paragraph takeaway)_

## Decision

_(to be filled after both: does span-first earn the break, or is the non-breaking EnumerateMatches swap the
right move for now?)_

Then **promote this decision into a first-class note** under `docs/notes/` (the repo's current-state memory)
so it's findable without reading the whole journal — a short note stating the recommendation, the numbers that
drove it, and the follow-up action (e.g. "apply the non-breaking `Match → EnumerateMatches` swap to the
shipping lexer" or "schedule span-first for v3"). Cross-link it back to this journal entry.
