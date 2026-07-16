---
title: Samples fail CA2225, CA1062, and CA1065
summary: All 37 fixed. CA2225 (16) and the operator-side CA1062 (12) went away structurally when the implicit operators were deleted; the 2 test-project CA1062 are suppressed by attribute; CA1065 (7) traced to a phantom Error enum state and was fixed by making it unrepresentable. Build 43 → 2.
tags: [note, lexi, analyzers, samples]
created: 2026-07-14
priority: low
effort: high
status: closed
blocked_by: "[[build-gate-is-red-after-unsuppressing-analyzers]]"
---

# Samples fail CA2225, CA1062, and CA1065

The bulk of the red gate — 37 of the 43 errors, almost all in `samples/Predicate/Predicate.Parser`. All resolved. The three rules turned out to have three different answers, which is why bundling them was the wrong shape for this note.

## CA2225 (16) — resolved structurally, not satisfied

Named alternates (`ToBoolean`/`FromBoolean` etc.) were written for all 16 sites first. Then Mark ruled implicit operators out entirely, the operators were deleted, and CA2225 stopped applying — the rule exists only to police operator overloads. The alternates survive as the actual API. See [[implicit-operators-removed-from-predicate-samples]].

The alternates were structured so the named method owns the logic and the operator delegated to it, rather than duplicating the body — which meant deleting the operators afterwards cost nothing.

## CA1062 (14) — 12 gone with the operators, 2 suppressed

All 12 sample sites were on implicit-operator parameters and vanished with them. The remaining 2 are in `tests/Lexi.Tests/LexiTests.cs:48,50` — a `[Theory]` whose `string` and `TestToken[]` parameters CA1062 wanted guarded. Suppressed by attribute with a justification: xUnit supplies both from `[InlineData]`, so null is unreachable and a guard would be dead code. The method is only `public` because xUnit requires it.

That is the case Mark named as "suppress with attribute and justification when it shouldn't be enforced". `LexiTests` already used exactly this pattern for CA1861.

Note the scaffolding-csharp template turns CA1062 **off** for `-t library` (non-packable) and on for `-t package`, so enforcing it in the samples at all was stricter than canon — a deliberate deviation.

## CA1065 (7) — the real bug

Not ceremony. Traced to an `Error = 0` member on three token enums that nothing ever constructed, forcing `ToString()` to carry a throw arm for an impossible case. Fixed by deleting the phantom state and validating at construction. Full write-up in [[ca1065-tostring-threw-on-a-phantom-state]].

## Result

Build went 43 → 2 errors; the 2 remaining are CA1724 ([[ca1724-parser-type-matches-namespace]]). All 51 tests pass (Lexi 23, Math.Parser 25, Predicate.Parser 3), so none of it changed behavior.

Two suppressions were added along the way, both for rules demanding a design the code deliberately rejects: CA1028 ([[ca1028-conflicts-with-symbol-tokenid]]) and CA1008.
