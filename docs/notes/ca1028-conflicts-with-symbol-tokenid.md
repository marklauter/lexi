---
title: CA1028 conflicts with Symbol.TokenId being uint
summary: CA1028 demanded enum : int, but token ids are an unsigned concept — Symbol.TokenId is uint and Pattern's EndOfSource flag is the sign bit. Ruling — the rule is wrong for this codebase and is suppressed solution-wide in Directory.Build.props. Build drops 43 → 39 errors.
tags: [note, lexi, analyzers, api, design]
created: 2026-07-14
priority: low
effort: low
status: closed
blocked_by: "[[build-gate-is-red-after-unsuppressing-analyzers]]"
---

# CA1028 conflicts with Symbol.TokenId being uint

4 of the (then) 43 errors. CA1028 (enum storage should be `Int32`) fired on:

- `samples/Predicate/Predicate.Parser/Expressions/ComparisonOperators.cs:3` — `public enum ComparisonOperators : uint`
- `samples/Predicate/Predicate.Parser/Expressions/LogicalOperators.cs:3` — `public enum LogicalOperators : uint`
- `samples/Predicate/Predicate.Parser/Expressions/NumericTypes.cs:6` — `public enum NumericTypes : uint`
- `tests/Lexi.Tests/TestToken.cs:3` — `public enum TestToken : uint`

These are `uint` because Lexi's public API is `uint`:

- `src/Lexi/Symbol.cs:38` — `public readonly uint TokenId = tokenId;`
- `src/Lexi/Symbol.cs:59` — `public bool Is(uint tokenId) => TokenId == tokenId;`
- `src/Lexi/Symbol.cs:43,51` — `TokenId` is bit-tested against `Pattern.NoMatch` / `Pattern.EndOfSource`.

The enum backing type is load-bearing, not incidental — it exists so a token enum drops straight into `Symbol.Is(uint)` without a cast.

## Ruling: suppressed, because the rule is wrong here

Mark's ruling: the backing type is the library author's call, and signed storage is the wrong shape for an inherently unsigned concept. CA1028 is rejected on the merits rather than worked around. Suppressed solution-wide in `Directory.Build.props` with a justification naming `Symbol.TokenId`; the build drops from 43 to 39 errors and no other rule is affected.

This is a deliberate, permanent deviation, not deferred work — there is no follow-up to schedule.

## Why this is a rejection and not a concession

The decisive fact is `Pattern.cs:18` — `EndOfSource = 1U << 31`. That is the sign bit. There is no non-negative `int` spelling of it, so the reserved-flag scheme is unsigned at the root, not by preference. `NoMatch` (`1U << 30`) sits directly beneath it. CA1028 exists to smooth interop and reflection over enums with unusual storage; it has nothing to say about a type whose whole point is to share a bit layout with an unsigned API.

Two things found while verifying that make the original note's framing worth correcting:

**The range argument never applied.** `Pattern.cs:64` rejects any token id `>= NoMatch`, so the legal range for a user token id is `0 … 2^30-1`, which fits inside `int` with a bit to spare (no sample or test value exceeds 1010). So "uint is needed for the range" would have been false — had that been the only reason, option 2 below would have been nearly free. The real reason is the flag bits, and that reason is airtight.

**The codebase already mixed both.** `samples/Predicate/Predicate.Parser/TokenIds.cs` declared `IDENTIFIER`, `FROM`, `WHERE`, `SKIP`, and `TAKE` as `const int` while every other constant in the same class was `const uint`, and it compiled — constant conversion handles it. So the strict claim "the enums must be `uint` to interop" was not quite true in the samples' own code. The `uint` is still right, but for the flag-layout reason, not an interop necessity.

That mix has since been cleaned up, and it turned out not to be untidiness — see below.

## The options that were rejected

1. **Cast at every call site.** Obeys CA1028, makes the samples noisier, and the samples exist to demonstrate ergonomics.
2. **Change `Symbol.TokenId` to `int`.** Would obey the rule at the root, and the range analysis above means the break is smaller than first estimated — but it forces the flag constants negative and inverts the domain's sign semantics to satisfy a style rule. Rejected as backwards.

## Follow-up: the int/uint mix was load-bearing, and hid a fifth enum

Cleaning up the mixed constants (all token ids are now `uint` in both samples) surfaced something the CA1028 census had missed. `samples/Predicate/Predicate.Parser/Expressions/Keywords.cs:3` is a **fifth** token enum, and it was `int`-backed:

```csharp
public enum Keywords          // no : uint
{
    Error = 0,
    From = TokenIds.FROM,     // const int
    ...
}
```

It compiled *only* because `FROM`/`WHERE`/`SKIP`/`TAKE` were `const int`. Promoting those constants to `uint` broke it with four `CS0266`s — so the inconsistency in `TokenIds.cs` was not cosmetic, it existed to keep this enum alive. Fixed by declaring `Keywords : uint` like its four siblings.

The instructive part: **CA1028 never fired on `Keywords`**, because `int` is exactly what the rule wants. The one token enum that was genuinely inconsistent with the design was the one the analyzer called compliant, while the four that were correct got flagged. That is the clearest evidence that the rule was measuring the wrong thing here.

Also fixed in the same pass: `samples/Math/Math.Parser/TokenIds.cs` was `const int` throughout — all 13 constants — so the two samples disagreed with each other about the token id type. Both are `uint` now. `Math`'s `NumericTypes` is deliberately left `int`-backed: it is a classification enum with its own numbering, not a token id, and does not alias `TokenIds` the way Predicate's does.

Incidental gain: `samples/Predicate/Predicate.Parser/Parser.cs:207` casts `(Keywords)matchResult.Symbol.TokenId`. Against an `int`-backed enum that cast wrapped negative for values above `int.MaxValue`; against `uint` it is lossless.

Build is unchanged at 39 errors (36 `Predicate.Parser`, 2 `Lexi.Tests`, 1 `Math.Parser`) with no `CS` errors, so the cleanup is behavior-neutral. Not confirmed by a test run — `Lexi.Tests` and `Math.Parser` still fail the gate on CA1062/CA1724, so `dotnet test` cannot build them yet. The constant values are unchanged and every comparison is now `uint`-to-`uint`, so a behavior change is not reachable; confirm when the gate goes green.

## Note on the no-suppression ruling

[[build-gate-is-red-after-unsuppressing-analyzers]] records the ruling that CA1028/CA1031/CA1062/CA1065/CA1724/CA2225 cannot be suppressed. CA1028 is now a carve-out from that. The distinction is between a rule catching real debt (CA1065 throwing from `ToString()` is a genuine flaw — see [[samples-fail-ca2225-ca1062-ca1065]]) and a rule fighting a correct design decision. The remaining five still stand unsuppressed.
