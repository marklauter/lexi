---
title: Math.REPL caught Exception because Math lacked a ParseException base
summary: The two samples were inconsistent — Predicate had a ParseException base class its exceptions derived from, so Predicate.REPL caught that narrow type; Math's exceptions derived straight from Exception, forcing Math.REPL into a catch-all that tripped CA1031. Fixed by giving Math the same base Predicate already had.
tags: [note, lexi, samples, design, analyzers]
created: 2026-07-15
priority: low
effort: low
status: closed
---

# Math.REPL caught Exception because Math lacked a ParseException base

Surfaced when the CA1724 namespace rename ([[ca1724-parser-type-matches-namespace]]) let `Math.REPL` build under the strict gate for the first time, tripping `CA1031` at `Program.cs:32`.

## Not a rule to suppress — a missing design the other sample already had

The two samples diverged:

- **Predicate** — `ParseException` is a non-sealed base; `UnexpectedTokenException` and `UnexpectedEndOfSourceException` derive from it. `Predicate.REPL` catches `ParseException` — one narrow type, no CA1031.
- **Math** — `UnexpectedTokenException` and `UnexpectedEndOfSourceException` derived **directly from `Exception`** with no common base. `Math.REPL` had nothing narrower to catch, so it caught `Exception`. That is the CA1031.

So CA1031 was not wrong here — Predicate's REPL proves a narrow catch works. Math was simply missing the base class. Suppressing would have preserved the inconsistency; the fix removes it.

## Fix

Mirrored Predicate's hierarchy into Math:

- Added `samples/Math/Math.Parsing/Exceptions/ParseException.cs` — identical shape to Predicate's base.
- Repointed `UnexpectedTokenException` and `UnexpectedEndOfSourceException` from `: Exception` to `: ParseException`.
- `Math.REPL/Program.cs` now `catch (ParseException ex)` with `using Math.Parsing.Exceptions;`.

Narrowing is safe: the only things `Math.REPL` runs are `parser.Parse` (throws the two parse exceptions) and `expression.Evaluate` (throws `NotSupportedException` only on an internal enum-mismatch bug; double division by zero yields `Infinity`, not an exception). Every legitimate user-input failure is a `ParseException`. A genuine internal bug now crashes the REPL instead of being swallowed, which is the correct fail-fast behavior — matching the writing-csharp rule that a catch-all belongs only at the host's outermost handler, and only when nothing narrower fits.

## Correction to the epic

[[build-gate-is-red-after-unsuppressing-analyzers]] recorded "CA1031 fires zero times ... the suppression was for a rule that never triggered." That was measured while both REPLs were unbuildable (blocked by the parser's CA1724), so CA1031 never got the chance to fire. The original samples suppression that named the REPL catch-all as CA1031's reason was correct. The epic is corrected accordingly.
