---
title: Apply Match→EnumerateMatches now (non-breaking); schedule span-first for v3
summary: A/B benchmarks settle the span-first spike. The non-breaking regex-API swap (regex.Match → regex.EnumerateMatches) captures the entire allocation headline — 5.2 MB / 52 MB → 0 B — plus ~12% speed, with no public-surface change; apply it to the shipping lexer now. The full span-first rewrite reaches a true end-to-end 0 B and ~27% faster, but its real justification is collectible/streamable tokens (Symbol as a plain record struct), so schedule it deliberately for v3 rather than rushing it for speed.
tags: [note, lexi, performance, regex, span, semver, decision]
created: 2026-07-16
priority: medium
effort: medium
status: open
---

# Apply Match→EnumerateMatches now (non-breaking); schedule span-first for v3

The `spike/span-first-lexer` branch ran two BenchmarkDotNet A/Bs to de-conflate the two changes a span-first
lexer bundles: the *regex-API swap* (non-breaking) and the *span-first type rewrite* (breaking). Full method
and per-experiment tables are in the journal: [[span-first-experiments]] (`docs/journal/span-first-experiments.md`).

## The numbers

| Experiment | Workload | Baseline alloc | New alloc | Speed (ratio) |
|------------|----------|---------------:|----------:|--------------:|
| 1 — cheap swap (non-breaking) | tokenize only | 5.2 MB / 52 MB | **0 B** | 0.87–0.88 (~12% faster) |
| 2 — full span-first (breaking) | tokenize **+ read text** | 5.55 MB / 55.5 MB | **0 B** | 0.73–0.74 (~27% faster) |

The baseline allocation splits into two parts: ~5.2 MB / 52 MB is the per-pattern-per-token
`System.Text.RegularExpressions.Match` object, and the extra ~0.35 MB / 3.5 MB is the substring `ReadSymbol`
allocates per token when a consumer reads the text.

## The recommendation

**Apply the cheap swap to the shipping lexer now.** Change `Pattern.Match` from `regex.Match(text, offset)`
to iterating `regex.EnumerateMatches(text.AsSpan(), offset)` and taking the first `ValueMatch`. This kills the
entire `Match`-object allocation (5.2 MB / 52 MB → 0 B) and buys ~12% speed with **zero** change to the public
surface — same `string` in, same token out, same algorithm, one line touched. It is a pure win; there is no
reason to defer it. Ships as a patch/minor.

**Schedule span-first for v3 — but for the API, not the speed.** The full rewrite reaches a true end-to-end
0 B and ~27% faster. Once the cheap swap is in, the *marginal* microbench gain of the break is modest: the
last ~6% of allocation on the read path (the `ReadSymbol` substring the `string` API can't avoid; `Slice`
returns a `ReadOnlySpan<char>` with no copy) and roughly another ~14 points of speed. What actually earns a
major version is the **type ergonomics**: with `Symbol` a plain `readonly record struct` carrying no span,
tokens become collectible and streamable — `List<Symbol>`, `IEnumerable<Symbol>`, across `await` — because the
span lives only in `Source`. The ref-struct wall that blocks every token-buffering consumer is gone. That is
the stated rationale for the break; the zero-alloc read path and extra speed are what make taking it free.

## Follow-up action

1. **Now:** apply `Match → EnumerateMatches` to `src/Lexi/Pattern.cs` on `main` (non-breaking). Guard it with
   the existing tests plus a MemoryDiagnoser check that per-token allocation is zero.
2. **v3:** land the span-first rewrite (the `src/Lexi.Spans` prototype), reusing `Slice`-over-`ReadSymbol` and
   the plain-struct `Symbol`. Fold under the v3 major bump — see [[lexi-3-0-0-is-a-breaking-release]] for the
   versioning ruling — with collectible/streamable tokens as the changelog headline.

The `bench/` project and `src/Lexi.Spans` stay off `Lexi.slnx` (out of the PR gate/coverage) until the v3
work is scheduled.
