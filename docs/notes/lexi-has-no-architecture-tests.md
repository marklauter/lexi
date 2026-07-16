---
title: Lexi has no architecture tests
summary: Added six ArchUnitNET rules inline at tests/Lexi.Tests/Architecture/ArchitectureTests.cs, following pool and dynamodblite rather than plumber's shared base project. One canon rule was adapted rather than copied, and the suite immediately caught VocabularyBuilder being unsealed.
tags: [note, lexi, testing, architecture, canon]
created: 2026-07-14
priority: low
effort: low
status: closed
---

# Lexi has no architecture tests

All three reference repos carry arch tests. Lexi carried none — the one part of the canonical pattern the modernization did not close.

The canon is not uniform on **how**:

- **plumber** — a shared `tests/Architecture.Testing/` base project with `ArchitectureTestsBase`, matching the scaffolding-csharp template.
- **pool** — no base project; a single sealed class at `tests/Pool.Tests/Architecture/ArchitectureTests.cs`.
- **dynamodblite** — same as pool.

The shared base is a plumber-only convention, not house canon. Mark's ruling: no separate project, embed in the existing test project — which is also the 2-of-3 majority and the lighter shape for a single-package repo.

## What was added

`tests/Lexi.Tests/Architecture/ArchitectureTests.cs`, inline, following pool's shape exactly: a static `ArchitectureModel` loaded from `typeof(Lexer).Assembly`, a private `Verify(IArchRule)` helper, one `[Fact]` per rule. `TngTech.ArchUnitNET` 0.13.3 pinned in `Directory.Packages.props` (same version as pool and dynamodblite) and referenced by name from `Lexi.Tests.csproj`.

Six rules: `AllTypesResideInLexiNamespaceTree`, `ConcreteClassesAreSealed`, `InstanceFieldsAreReadOnly`, and the three dependency bans (`AspNetCore`, `Hosting`, `Console`).

Per the scaffolding-csharp skill, `WithoutRequiringPositiveResults()` is not used anywhere — Lexi has real types from day one, so no rule can pass vacuously.

## One rule is a deliberate deviation from canon

Pool and dynamodblite both assert `InstanceFieldsAreNotPublic`. **That rule cannot be adopted verbatim** — Lexi's public surface is ref structs that expose public instance fields by design:

- `Symbol.Offset`, `Symbol.Length`, `Symbol.TokenId`
- `Source.Offset`
- `MatchResult.Source`, `MatchResult.Symbol`
- `Lexer`'s nested `Symbol`/`Index`

`Symbol` even carries a `CA1051` suppression saying so (*"it's a struct"*). The fields exist to keep the lexer's hot path allocation-free.

The invariant pool is protecting is *immutability* — its own comment says "no public mutable instance state". Lexi honours that: every one of those fields is `readonly`. So the rule was adapted to `InstanceFieldsAreReadOnly` (`Should().BeReadOnly()`), which asserts the same invariant against a codebase shaped differently. The deviation is commented at the rule.

## It caught something immediately

`ConcreteClassesAreSealed` failed on first run:

```
Lexi.VocabularyBuilder is not sealed
```

`src/Lexi/VocabularyBuilder.cs:9` was `public class` while every other concrete type in the library (`Lexer`, `Pattern`) is `sealed`. Fixed by sealing it.

**Sealing is normally a breaking change, but this one is provably zero-impact**: `VocabularyBuilder`'s only constructor is `private` (`:17`), and instances come from the static `Create()` factories. No consumer can ever have derived from it, because no derived class could call the constructor. It was effectively sealed already and simply missing the keyword — which is exactly the kind of drift an arch test is for.

## Result

57 tests pass (Lexi.Tests 29, Math.Parser.Tests 25, Predicate.Parser.Tests 3). Coverage unchanged at 82.67/70.45/72.97 — the arch tests exercise `Lexi.dll`'s metadata rather than its code, so they do not move the ratchet. See [[coverage-floor-is-below-the-house-standard]].

## Worth adding later

The rulings made during this review are prose in `docs/notes/` but not yet enforced. Two are natural arch rules:

- **No implicit conversion operators** (`MethodMembers().That().HaveName("op_Implicit").Should().NotExist()` or equivalent). Deliberately **not** added yet — it would fail today on `src/Lexi/Source.cs:65,79`, which is still an open decision. See [[source-implicit-operators-are-the-last-holdout]]. Add the rule as part of that change and it locks the ruling in.
- **Token enums are `uint`-backed**, encoding [[ca1028-conflicts-with-symbol-tokenid]] so the CA1028 suppression cannot quietly become drift.
