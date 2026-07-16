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

Full job, `[MemoryDiagnoser]`, net10.0, Release. Correctness gate passed (byte-for-byte identical extracted
text, hence identical content hash). Both arms tokenize **and** read each token's text.

| Method             | Repeats | Mean      | Ratio | Allocated  | Alloc Ratio |
|------------------- |-------- |----------:|------:|-----------:|------------:|
| ShippingReadSymbol |   1,000 |  2.426 ms |  1.00 |  5,552,000 B |      1.00 |
| SpanSlice          |   1,000 |  1.779 ms |  0.73 |          0 B |      0.00 |
| ShippingReadSymbol |  10,000 | 24.143 ms |  1.00 | 55,520,000 B |      1.00 |
| SpanSlice          |  10,000 | 17.967 ms |  0.74 |          0 B |      0.00 |

**Takeaway:** end-to-end, span-first is **0 B** (from 5.55 MB / 55.5 MB) and **~27% faster** (ratio
0.73–0.74). Decompose the baseline allocation: ~5.2 MB / 52 MB is the per-pattern-per-token `Match` object
(the part exp 1's cheap swap already kills), and the extra ~0.35 MB / 3.5 MB is the substring `ReadSymbol`
allocates per token on the read path. The cheap swap **cannot** remove that residual — it keeps the `string`
surface, so a text-reading consumer still allocates a substring per token. Only the span rewrite's `Slice`
(a `ReadOnlySpan<char>`, no copy) takes the read path to a true zero. The marginal value of the break over
the cheap swap is therefore: (a) the last ~6% of allocation that lives on the read path → 0 B, and (b) the
extra speed — the end-to-end ratio 0.73 vs the cheap swap's tokenize-only 0.87, because `Slice` never copies.
The number the break most has to justify, though, isn't in this table: making `Symbol` a plain record struct
dissolves the ref-struct wall, so tokens become collectible/streamable (`List<Symbol>`, `IEnumerable<Symbol>`,
across `await`). That API change is the real prize; the zero-alloc read path and the extra speed are what make
it free to take.

### Follow-up — can polish push span-first past ~27%?

**Question:** the two changes are already both present in the exp-2 span arm (`Lexi.Spans.Pattern` uses
`EnumerateMatches` *and* the span-first types), so ~27% is "both" combined, not two wins that stack. Is there
any further headroom from bringing the prototype up to the shipping lexer's `[MethodImpl(AggressiveInlining)]`
discipline (which `Lexi.Spans` originally lacked entirely)?

Added `AggressiveInlining` to the span-first hot path (`Source` ctor/`Slice`, `Symbol.Is`, `Pattern.Match`,
`Lexer.NextMatch`/`SkipIgnored`) and re-ran exp 2:

| Method             | Repeats | Mean      | Ratio | Allocated | Alloc Ratio |
|------------------- |-------- |----------:|------:|----------:|------------:|
| ShippingReadSymbol |   1,000 |  2.422 ms |  1.00 | 5,552,000 B |      1.00 |
| SpanSlice (inlined)|   1,000 |  1.751 ms |  0.72 |         0 B |      0.00 |
| ShippingReadSymbol |  10,000 | 24.097 ms |  1.00 | 55,520,000 B |     1.00 |
| SpanSlice (inlined)|  10,000 | 17.886 ms |  0.74 |         0 B |      0.00 |

**Result: no.** The ratio holds at **0.72–0.74** — indistinguishable from the pre-inlining 0.73–0.74. ~27% is
the ceiling for this algorithm, and inlining is within measurement noise, because the workload is **regex-
bound**: both arms perform the *same* number of `\G`-anchored regex operations per token (one whitespace skip
+ one per match pattern until one wins), so wall-clock is dominated by the regex engine, not by the token/slice
plumbing that inlining touches. The span-first win is entirely the eliminated allocation and its GC pressure;
the compute is unchanged. Beating ~27% would require *reducing regex work* — e.g. a single combined
alternation regex (but `ValueMatch` exposes no group id, so recovering token identity forces the allocating
`regex.Match` back in) or replacing the regex engine with a hand-rolled character scanner. Both are
architecture changes well beyond "apply the swap + span types", and out of scope for this spike. The
inlining is kept anyway as promotion-prep — it matches the shipping lexer's discipline and does no harm.

## Decision

**Do both, sequenced — the non-breaking swap now, the span-first break for v3.**

1. **Apply `Match → EnumerateMatches` to the shipping lexer now (non-breaking).** Exp 1 shows this captures
   the entire headline — the per-pattern-per-token `Match` allocation (5.2 MB / 52 MB → **0 B**) — plus
   ~12–13% speed, with *no* change to the public surface. It is the same `string` in, the same token out, the
   same algorithm; the only edit is one line in `Pattern.Match`. There is no reason to defer a pure win that
   costs nothing. This is the immediate move.

2. **Span-first earns the v3 break, but on the strength of the API, not raw speed.** Exp 2 shows the full
   rewrite reaches a true end-to-end **0 B** and **~27%** faster. But once the cheap swap is in, the
   *marginal* microbench delta of the break is modest: the extra ~6% of allocation that lives on the read
   path (the `ReadSymbol` substring the `string` API can't avoid) and roughly another ~14 points of speed
   from `Slice` not copying. What actually justifies a breaking change is the **type ergonomics**: `Symbol`
   as a plain `readonly record struct` carries no span, so tokens become collectible and streamable
   (`List<Symbol>`, `IEnumerable<Symbol>`, across `await`) — the ref-struct wall that blocks every
   token-buffering consumer is gone. That is a design-level improvement worth a major version; the zero-alloc
   read path and the added speed are what make taking it free rather than a trade-off.

**Net:** ship the cheap swap immediately as a patch/minor; schedule span-first deliberately for v3 with the
collectible-token API as its stated rationale, backed by the end-to-end zero-alloc + ~27% numbers above. Do
not rush the break for speed alone — the swap already banks the allocation headline.

Then **promote this decision into a first-class note** under `docs/notes/` (the repo's current-state memory)
so it's findable without reading the whole journal — a short note stating the recommendation, the numbers that
drove it, and the follow-up action (e.g. "apply the non-breaking `Match → EnumerateMatches` swap to the
shipping lexer" or "schedule span-first for v3"). Cross-link it back to this journal entry.
