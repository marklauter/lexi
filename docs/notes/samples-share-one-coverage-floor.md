---
title: Samples shared one coverage floor and masked regressions
summary: samples/Directory.Build.props set a single 68,36,61 floor for both sample suites, taking the minimum of each axis. Math.Parsing's 64% branch coverage was gated at Predicate's 37%, so a 27-point regression there passed CI silently. Fixed with per-project floors seeded at each suite's measured value.
tags: [note, lexi, coverage, testing, samples]
created: 2026-07-14
priority: medium
effort: low
status: closed
---

# Samples shared one coverage floor and masked regressions

`samples/Directory.Build.props` set `<Threshold>68,36,61</Threshold>` for every `*.Tests` project under `samples/`, seeded at the lower of the two suites on each axis:

- `Math.Parsing` measured 68.04 / **64.47** / 74.28
- `Predicate.Parsing` measured 69.76 / **37.98** / 64.93

Because the shared floor took the minimum per axis, Math.Parsing's branch coverage was gated at ~37% when it actually sits at ~64%. Failure scenario: someone weakens Math.Parsing's branch tests, coverage drops from 64% to 38%, and CI stays green.

## Fix

Per-project floors, keyed on `$(MSBuildProjectName)` in `samples/Directory.Build.props`, each seeded at its own measured line,branch,method (floored to integer, matching the library floor's convention):

```xml
<PropertyGroup Condition="'$(MSBuildProjectName)' == 'Math.Parsing.Tests'">
  <Threshold>68,64,74</Threshold>
</PropertyGroup>
<PropertyGroup Condition="'$(MSBuildProjectName)' == 'Predicate.Parsing.Tests'">
  <Threshold>69,37,64</Threshold>
</PropertyGroup>
```

Keeping all sample coverage policy in the one props file mirrors how the library floor lives in the root `Directory.Build.props` rather than in `Lexi.Tests.csproj`. Plumber's `Directory.Build.props` explicitly sanctions per-project overrides, so this is within canon. A new sample `.Tests` project seeds its own block after its first green run (a comment in the file says so).

Numbers were re-measured against the current samples, not taken from the seeding note — the implicit-operator removal and named factories moved Predicate's branch from 36.52 to 37.98 and its method from 61.84 to 64.93. Math was unchanged.

## Verified

Both suites pass at their own floors. The fix was confirmed to bite: forcing Math.Parsing's branch floor to 65 (`-p:Threshold=65`) now fails with *"The total branch coverage is below the specified 65"* — under the old shared 36 floor, a drop to any value above 36 passed silently. The regression the note described can no longer slip through.

Related: [[coverage-floor-is-below-the-house-standard]] (the library floor sits below the 95 house standard — still open, still needs tests) and [[thresholdstat-total-vs-minimum-disagreement]] (whether these floors should gate per-class instead of aggregate — still open).
