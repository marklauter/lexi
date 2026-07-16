---
title: Implicit operators removed from the Predicate samples
summary: Mark's ruling — implicit operators hide bugs and are no longer used. All 14 were deleted from the Predicate expression types in favour of named factories and ToString(). This killed CA2225 (16), CA1062 (12), and CA1065 (7) structurally. src/Lexi/Source.cs still has two and is a separate decision.
tags: [note, lexi, api, design, analyzers, samples, breaking-change]
created: 2026-07-15
priority: low
effort: low
status: closed
---

# Implicit operators removed from the Predicate samples

Mark's ruling: *"I no longer use implicit operators because of the kind of issues we're having here. I used to think they were helpful, but now I see they hide bugs."* Reference for the modern shape is kingo's `Values` and `Results` projects (`D:\projects\kingo\kingo\src\Values`, `src\Results`).

## The finding that made this cheap

**The implicit operators were never used implicitly.** Every one of the 12 conversion sites in the Predicate sample was already written as an explicit cast:

```csharp
(ComparisonOperator)matchResult.Symbol.TokenId
(NumericLiteral)int.Parse(...)
(BooleanLiteral)false
(string)identifier
```

So the implicitness bought nothing at the call sites that existed — it only meant the compiler *would also* convert silently anywhere someone later slipped. That is exactly the bug-hiding Mark named, and it made deletion a mechanical 1:1 swap rather than a redesign.

## What replaced them

Per kingo's `IValue<TSelf, TValue>`: a named factory for construction, `ToString()` for the canonical text form (the inverse of the parse), and no conversion operators at all.

| was | now |
|---|---|
| `(Keyword)tokenId` | `Keyword.FromUInt32(tokenId)` |
| `(ComparisonOperator)tokenId` | `ComparisonOperator.FromUInt32(tokenId)` |
| `(Identifier)value` | `Identifier.FromString(value)` |
| `(StringLiteral)text` | `StringLiteral.FromString(text)` |
| `(CharacterLiteral)text` | `CharacterLiteral.FromString(text)` |
| `(NumericLiteral)int.Parse(...)` | `NumericLiteral.FromInt32(int.Parse(...))` |
| `(NumericLiteral)double.Parse(...)` | `NumericLiteral.FromDouble(double.Parse(...))` |
| `(BooleanLiteral)false` | `BooleanLiteral.FromBoolean(false)` |
| `(string)identifier` | `identifier.ToString()` |

`ToString()` is now overridden on `BooleanLiteral`, `CharacterLiteral`, `Identifier`, `StringLiteral`, and `NumericLiteral` to emit canonical text. Previously only `Keyword` had it, which is why `StatementPrinter` mixed `keyword.ToString()` with `(string)identifier` in adjacent switch arms — that inconsistency is gone.

## Analyzer effect

CA2225 exists only to police operator overloads; CA1062 was firing on operator parameters. Deleting the operators removed both **structurally** rather than by satisfying them:

| rule | before | after |
|---|---|---|
| CA2225 | 16 | 0 |
| CA1062 | 14 | 0 (2 in `Lexi.Tests` suppressed by attribute) |
| CA1065 | 7 | 0 (see [[ca1065-tostring-threw-on-a-phantom-state]]) |

Build went 43 → 2 errors. Both remaining are CA1724, see [[ca1724-parser-type-matches-namespace]].

All 51 tests pass (Lexi 23, Math.Parser 25, Predicate.Parser 3), so the removal is behavior-neutral.

## Still open: src/Lexi/Source.cs

`src/Lexi/Source.cs:65,79` still declares two implicit operators, and this is the **published library**, not a sample:

```csharp
public static implicit operator string(Source script) => script.text;
public static implicit operator Source(string source) => new(source);
```

Both already have named alternates beside them (`ToString()` at :72, `FromString(string)` at :87) — the modernization added those to satisfy CA2225 — so deleting the operators is a two-line change.

**But the asymmetry matters.** Unlike the samples, `Source`'s `string`→`Source` operator *is* genuinely used implicitly: `tests/Lexi.Tests/LexiTests.cs:31` calls `lexer.NextMatch(source)` with a `string`, relying on the conversion. `Lexer.NextMatch` only overloads on `Source` and `MatchResult`. So removing it:

- breaks any consumer calling `lexer.NextMatch("some text")` — they would need `Source.FromString("some text")`,
- costs a real ergonomic that the sample operators never delivered.

This is a judgement call about the published API rather than a cleanup, so it is left for Mark. It is free to take inside the major bump ([[lexi-3-0-0-is-a-breaking-release]]) if wanted. Tracked in [[source-implicit-operators-are-the-last-holdout]].
