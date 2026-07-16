---
title: Build gate is red after unsuppressing analyzers
summary: The six suppressed rules were removed on request, failing the build with 43 analyzer errors. All resolved. CA2225/CA1062/CA1065 fixed, CA1028/CA1008 rejected and suppressed, CA1724 fixed by namespace rename, and CA1031 — which this note wrongly called dead — was real, masked by the unbuildable REPLs, now fixed. Build, 57 tests, and format are green.
tags: [note, epic, lexi, analyzers, build-gate]
created: 2026-07-14
priority: high
effort: high
status: closed
---

# Build gate is red after unsuppressing analyzers

Mark's ruling: CA1028/CA1031/CA1062/CA1065/CA1724/CA2225 cannot be suppressed — a failing gate is preferable to a `NoWarn`. Both suppression sites were removed:

- `samples/Directory.Build.props` — the whole six-rule `NoWarn` and its justification block.
- `Directory.Build.props` — `CA1028` and `CA1062` dropped from the test-project carve-out (now `:52`), leaving `CA1707;IDE1006;IDE0079;CA1515` (which matches the scaffolding-csharp template's test carve-out).

**CA1028 has since been carved back out** as a solution-wide suppression (`Directory.Build.props:26`) — see below and [[ca1028-conflicts-with-symbol-tokenid]]. The ruling stands for the other five. The line it draws is between a rule catching real debt and a rule fighting a correct design decision.

`dotnet build "Lexi.slnx" -c Debug` originally failed with **43 errors** under `TreatWarningsAsErrors`. It is now at **2**:

| rule | was | now | outcome |
|---|---|---|---|
| CA2225 | 16 | 0 | gone structurally — the implicit operators it policed were deleted |
| CA1062 | 14 | 0 | 12 gone with the operators; 2 suppressed by attribute in `Lexi.Tests` |
| CA1065 | 7 | 0 | real bug — a phantom `Error = 0` enum state, made unrepresentable |
| CA1028 | 4 | 0 | rule rejected, suppressed with justification |
| CA1724 | 2 | 0 | namespace rename `<Sample>.Parser` → `<Sample>.Parsing` |
| CA1031 | 0* | 0 | *see correction below — it was masked, not absent |

CA1724 is resolved via [[ca1724-parser-type-matches-namespace]].

## Correction: CA1031 was real, not dead

This note originally claimed **"CA1031 fires zero times ... the suppression was for a rule that never triggered."** That was wrong, and the way it was wrong is instructive.

`grep -c CA1031` returned 0 — but only because both sample REPLs were *unbuildable*: each depends on its parser project, the parsers carried the CA1724 error, so the REPLs never compiled and their analyzers never ran. The moment CA1724 was fixed and `Math.REPL` built, CA1031 fired at `Program.cs:32` exactly where the original samples suppression said it would. The suppression that named the REPL catch-all was correct all along.

Fixed without suppression by giving Math the `ParseException` base class Predicate already had, so the REPL catches a narrow type. See [[math-repl-lacked-a-parse-exception-base]].

The general lesson, twice over in this epic: an error count taken from a build where some projects fail to compile is a floor, not a total. The same masking hid the test-project errors early on.

Two rules were rejected rather than obeyed, both demanding a design the code deliberately refuses:

- **CA1028** — token ids are an unsigned concept; `Pattern.EndOfSource` is `1U << 31`, the sign bit. See [[ca1028-conflicts-with-symbol-tokenid]].
- **CA1008** — asked for the phantom `Error = 0` back after CA1065 was fixed by removing it. See [[ca1065-tostring-threw-on-a-phantom-state]].

Both are carve-outs from the ruling below, in `Directory.Build.props:34`.

All 51 tests pass (Lexi 23, Math.Parser 25, Predicate.Parser 3), so none of the fixes changed behavior.

> **Counting note.** These figures were originally recorded as 86 / 32 / 28 / 14 / 8 / 4, exactly double, and were corrected on review. MSBuild emits each diagnostic **twice** to the console — once inline as it occurs, once again in the end-of-build summary — so `grep -c 'error CA'` over a build log double-counts every violation. The reliable figures are MSBuild's own `43 Error(s)` tally, or a dedupe on file+line+rule:
>
> ```sh
> dotnet build Lexi.slnx -c Debug 2>&1 \
>   | grep -oE '[^ ]+\.cs\([0-9]+,[0-9]+\): error CA[0-9]+' | sort -u | wc -l
> ```
>
> The tell was internal: every child note's *detail list* was right while its headline count was doubled — [[ca1028-conflicts-with-symbol-tokenid]] said "8" and enumerated 4 enums, [[ca1724-parser-type-matches-namespace]] said "4" and listed 2 files. The prose was written from the violations; only the counts came from the log. Recount by dedupe, never by `grep -c`.

**CA1031 — see the correction above.** This paragraph originally read "CA1031 fires zero times ... the suppression was for a rule that never triggered." That was an artifact of the REPLs being unbuildable. It was a real violation, now fixed.

Children — all closed:

- [[samples-fail-ca2225-ca1062-ca1065]] — all 37 resolved.
- [[ca1065-tostring-threw-on-a-phantom-state]] — the one that was a real bug.
- [[ca1028-conflicts-with-symbol-tokenid]] — rule rejected and suppressed.
- [[ca1724-parser-type-matches-namespace]] — fixed by namespace rename.
- [[math-repl-lacked-a-parse-exception-base]] — the masked CA1031, fixed.

Related work that fell out of this: [[implicit-operators-removed-from-predicate-samples]] and its remaining holdout [[source-implicit-operators-are-the-last-holdout]].

**Closed.** `dotnet build`, all 57 tests, and `dotnet format --verify-no-changes` are green. Two rules stand suppressed with justification (CA1028, CA1008), both rejected on the merits rather than worked around.
