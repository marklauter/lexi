---
title: Lexi has no architecture tests
summary: pool, plumber, and dynamodblite all carry ArchUnitNET architecture tests in some form. Lexi has none. Plumber uses a shared Architecture.Testing base project; pool and dynamodblite inline a sealed ArchitectureTests per test project.
tags: [note, todo, lexi, testing, architecture, canon]
created: 2026-07-14
priority: medium
effort: medium
status: open
---

# Lexi has no architecture tests

All three reference repos carry arch tests. Lexi carries none. This is the one part of the canonical pattern the modernization did not close.

The canon is not uniform on **how**:

- **plumber** — a shared `tests/Architecture.Testing/` base project with `ArchitectureTestsBase`, matching the scaffolding-csharp template.
- **pool** — no base project; a single sealed class at `tests/Pool.Tests/Architecture/ArchitectureTests.cs`.
- **dynamodblite** — same as pool, at `tests/DynamoDbLite.Tests/Architecture/ArchitectureTests.cs`.

So the shared base is a plumber-only convention, not house canon. The 2-of-3 majority is the inline form, and it is the lighter one for a single-package repo like lexi.

Recommended shape: `tests/Lexi.Tests/Architecture/ArchitectureTests.cs`, inline, following pool.

Per the scaffolding-csharp skill, the base rules ship with `WithoutRequiringPositiveResults()` so an empty scaffold stays green — drop it from the rules that should always have subjects (`AllTypesResideInRootNamespace`, `ConcreteClassesAreSealed`) once real types exist, so they cannot pass vacuously. Lexi has real types from day one, so those rules should be written without it.

This is new test content rather than a build-configuration change, which is why it was left out of the modernization commit.
