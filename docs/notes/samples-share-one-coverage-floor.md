---
title: Samples share one coverage floor and mask regressions
summary: samples/Directory.Build.props sets a single 68,36,61 floor for both Math.Parser and Predicate.Parser. Math.Parser's real branch coverage is 64% but is gated at Predicate's 36%, so a 28-point regression there passes CI silently.
tags: [note, todo, lexi, coverage, testing, samples]
created: 2026-07-14
priority: medium
effort: low
status: open
---

# Samples share one coverage floor and mask regressions

`samples/Directory.Build.props` sets `<Threshold>68,36,61</Threshold>` for every `*.Tests` project under `samples/`. It was seeded at the lower of the two suites on each axis:

- `Math.Parser` measured 68.04 / **64.47** / 74.28
- `Predicate.Parser` measured 68.15 / **36.52** / 61.84

Because the floor takes the minimum of each axis across both, Math.Parser's branch coverage is gated at 36% when it actually sits at 64%. Failure scenario: someone deletes or weakens Math.Parser's branch tests, coverage drops from 64% to 37%, and CI stays green.

Fix is per-project floors — either a `Threshold` in each sample test csproj, or a condition on `$(MSBuildProjectName)` in `samples/Directory.Build.props`. Plumber's `Directory.Build.props` explicitly sanctions per-project overrides, so this is within canon.

Low effort, and it closes a real hole. Related: [[coverage-floor-is-below-the-house-standard]].
