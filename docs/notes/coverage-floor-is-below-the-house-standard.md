---
title: Coverage floor is far below the house standard
summary: pool, plumber, and dynamodblite all floor line coverage at 95. Lexi seeds 82/70/72. Nothing was lowered — no ratchet existed before — but the gap says lexi needs more tests.
tags: [note, todo, lexi, coverage, testing]
created: 2026-07-14
priority: medium
effort: high
status: resolved
---

> **Resolved 2026-07-16.** Tests written across the whole system; floors ratcheted to measured and holding.
> `Lexi` 82.67/70.45/72.97 → **100/97.72/100**. `Math.Parsing` 68.04/64.47/74.28 → **99.48/97.36/100**.
> `Predicate.Parsing` 69.76/37.98/64.93 → **98.79/92.8/100** (Release; branch coverage is config-dependent and CI
> runs Release, so floors are seeded from the strictest config). Two real defects surfaced en route, both in the
> Predicate sample: (1) `ParseComparison` reported a truncated condition as `unexpected token 'EOF'` instead of
> `UnexpectedEndOfSourceException` like every other parse step — fixed with a leading `CheckEndOfSource`; (2)
> `true`/`false` were unreachable as literals because the `Identifier` pattern was registered ahead of them and
> won the lowest-index tie-break — fixed by registering the keyword literals first (mirroring `null`). Residual
> sub-100 branch coverage is unreachable defensive code (guarded `ToString` defaults, a defensive tie-break arm,
> unimplemented array/object literal arms), documented at each ratchet site.

# Coverage floor is far below the house standard

`Directory.Build.props:34-36` seeds the ratchet at `82,70,72` (line, branch, method), measured from the first green run.

The reference repos:

| repo | threshold | stat |
|---|---|---|
| pool | 95,90,95 | total |
| dynamodblite | 95,85,95 | total |
| plumber | 95,95,95 | minimum |
| scaffolding template | 0,0,0 (seed) | minimum |

Lexi is 13 points below the house line floor and 20 below on branch. This is **not** a lowering — the parent branch had no ratchet at all, so seeding at measured is correct per the scaffolding-csharp skill ("raise `<Threshold>` to the measured line,branch,method after the first green run. Never lower it."). The honest read is that lexi is under-tested relative to the other libraries.

Measured coverage at the time of seeding: `Lexi` 82.67/70.45/72.97.

The work is writing tests to close the gap, then ratcheting up. The ratchet is verified live — forcing `-p:Threshold=99` errors with "The total line coverage is below the specified 99" — so it will hold the line once raised.

See [[samples-share-one-coverage-floor]] for the related problem in `samples/`, and [[thresholdstat-total-vs-minimum-disagreement]] for the stat choice.
