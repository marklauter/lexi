---
title: The reference repos disagree on ThresholdStat
summary: pool and dynamodblite use ThresholdStat total; plumber and the scaffolding template use minimum. Lexi picked total, the weaker of the two. The canon itself is inconsistent, so the house standard needs settling.
tags: [note, todo, lexi, coverage, canon, house-standard]
created: 2026-07-14
priority: low
effort: low
status: open
---

# The reference repos disagree on ThresholdStat

Surfaced while verifying lexi against the canonical NuGet-library pattern. The three source-of-truth repos do not agree:

| repo | ThresholdStat |
|---|---|
| pool | total |
| dynamodblite | total |
| plumber | minimum |
| scaffolding-csharp template | minimum |

`Directory.Build.props:36` in lexi uses `total`, following pool.

The difference is real: `total` gates the assembly aggregate, `minimum` gates per-class. The template's own comment describes `minimum` as "per-class minimum: every class must clear the floor." Under `total`, one well-covered class masks an untested one; under `minimum`, it cannot.

With `<Include>[Lexi]*</Include>` scoping coverage to a single module, the two differ only in per-class strictness, not in aggregation across modules.

This is a house-standard question rather than a lexi question — the template and plumber say one thing, pool and dynamodblite say another. Settling it means picking one and bringing the outliers in line, which touches all four repos. Recorded here because lexi is where the inconsistency surfaced.
