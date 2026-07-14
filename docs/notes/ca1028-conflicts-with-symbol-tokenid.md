---
title: CA1028 conflicts with Symbol.TokenId being uint
summary: CA1028 demands enum : int, but the sample and test token enums are uint because Lexi's own Symbol.TokenId is uint. Obeying the rule means casting at every call site or changing Symbol.TokenId to int — a bigger breaking change than the one it would fix.
tags: [note, todo, lexi, analyzers, api, design]
created: 2026-07-14
priority: high
effort: medium
status: open
blocked_by: "[[build-gate-is-red-after-unsuppressing-analyzers]]"
---

# CA1028 conflicts with Symbol.TokenId being uint

8 of the 86 errors. CA1028 (enum storage should be `Int32`) fires on:

- `samples/Predicate/Predicate.Parser/Expressions/ComparisonOperators.cs:3` — `public enum ComparisonOperators : uint`
- `samples/Predicate/Predicate.Parser/Expressions/LogicalOperators.cs:3` — `public enum LogicalOperators : uint`
- `samples/Predicate/Predicate.Parser/Expressions/NumericTypes.cs:6` — `public enum NumericTypes : uint`
- `tests/Lexi.Tests/TestToken.cs:3` — `public enum TestToken : uint`

These are `uint` because Lexi's public API is `uint`:

- `src/Lexi/Symbol.cs:38` — `public readonly uint TokenId = tokenId;`
- `src/Lexi/Symbol.cs:59` — `public bool Is(uint tokenId) => TokenId == tokenId;`
- `src/Lexi/Symbol.cs:43,51` — `TokenId` is bit-tested against `Pattern.NoMatch` / `Pattern.EndOfSource`.

The enum backing type is load-bearing, not incidental — it exists so a token enum drops straight into `Symbol.Is(uint)` without a cast.

Three ways out, none free:

1. Cast at every call site. Obeys CA1028, makes the samples noisier, and the samples exist to demonstrate ergonomics.
2. Change `Symbol.TokenId` to `int`. Obeys the rule at the root, but it is a larger break to `MSL.Lexi` than [[commonpatterns-static-breaks-consumers]], and the bit-flag usage against `Pattern.NoMatch`/`Pattern.EndOfSource` wants unsigned semantics.
3. Suppress CA1028 with a justification naming `Symbol.TokenId`. Contradicts the no-suppression ruling, but this is the case where the rule is arguably wrong for the codebase.

Needs a decision from Mark. The ruling was no suppressions; this note exists because CA1028 is the rule where that ruling has a real cost.
