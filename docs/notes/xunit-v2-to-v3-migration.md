---
title: Migrated from xunit v2 to xunit.v3
summary: Central Package Management forced one version per package id, and the repo's test projects were already split between v2 and v3-only dependencies. Unified on xunit.v3 to match pool. All 51 tests pass unchanged. Approved.
tags: [note, todo, lexi, testing, dependencies]
created: 2026-07-14
priority: medium
effort: medium
status: closed
---

# Migrated from xunit v2 to xunit.v3

Not part of the original modernization ask; forced by adopting Central Package Management.

CPM allows one version per package id across the solution, and the test projects were already incoherent:

- `Math.Parser.Tests.csproj` pinned `Xunit.DependencyInjection` 11.3.0 — **v3-only** — against `xunit` 2.9.3 and a v3 runner.
- `Lexi.Tests` and `Predicate.Parser.Tests` were pure v2.

There was no single consistent v2 pin available, so the choice was to unify up or down. Unified on v3 to match pool's `Directory.Packages.props`.

The cascade, every entry a major jump and every one matching pool exactly:

| package | from | to |
|---|---|---|
| xunit | 2.9.3 | xunit.v3 3.2.2 |
| xunit.runner.visualstudio | 2.8.0 | 3.1.5 |
| Xunit.DependencyInjection | 10.8.0 | 11.3.0 |
| Microsoft.NET.Test.Sdk | 17.13.0 | 18.7.0 |

## Resolution

Approved by Mark. All 51 tests pass with no test-source changes beyond a CA1307 fix (`Assert.Contains` given `StringComparison.Ordinal`) — verified against a static `[Fact]`/`[Theory]`/`[InlineData]` census across both branches, which came out identical file-for-file. No test was lost, skipped, or weakened.

The parent branch ran the same 51 tests across 3 target frameworks; the drop to a single run of 51 is the requested single-target, not lost coverage.
