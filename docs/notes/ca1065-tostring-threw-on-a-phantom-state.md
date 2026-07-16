---
title: CA1065 — ToString threw on a phantom Error state
summary: All 7 CA1065 sites traced to an Error = 0 member on three token enums that nothing ever constructed. Deleting it and validating at construction made ToString total. This tripped CA1008, which asks for the phantom back; CA1008 is suppressed.
tags: [note, lexi, analyzers, design, samples]
created: 2026-07-15
priority: low
effort: medium
status: closed
blocked_by: "[[build-gate-is-red-after-unsuppressing-analyzers]]"
---

# CA1065 — ToString threw on a phantom Error state

CA1065 (do not raise exceptions in unexpected locations) fired 7 times. Mark's read was right: this one was not ceremony, it was catching a real design flaw. Throwing from `ToString()` means a debugger inspecting a malformed AST node crashes.

## Root cause: a state nothing could produce

All of it traced to one mistake repeated three times:

```csharp
public enum Keywords : uint { Error = 0, From = TokenIds.FROM, ... }
public enum ComparisonOperators : uint { Error = 0, Equal = TokenIds.EQUAL, ... }
public enum LogicalOperators : uint { Error = 0, And = TokenIds.LOGICAL_AND, ... }
```

**Nothing ever constructed an `Error` value.** Every factory either maps a known token id or throws; `Error` existed only to be rejected in `ToString()`. It was a representable state with no producer — so each `ToString()` carried a throw arm for a case that could not arise, and the analyzer correctly flagged the throw.

## Fix

Deleted `Error = 0` from all three enums, then landed the invariant at construction so `ToString()` can trust its input rather than re-check it:

```csharp
public Keywords Value { get; } = Enum.IsDefined(Value)
    ? Value
    : throw new ArgumentOutOfRangeException(nameof(Value));
```

Applied to `Keyword.Value`, `ComparisonOperator.Value`, `ComparisonExpression.Operator`, `LogicalExpression.Operator`, and `NumericLiteral.Type`. Throwing from a constructor is correct — construction is where invariants land, and misuse is the caller's defect. `ToString()`'s final arm became `_ => Value.ToString()`: unreachable for a validated instance, and total rather than throwing.

All three enums were explicitly valued from `TokenIds`, so removing the zero member renumbered nothing.

## Three of the seven had gone dark first — worth recording

The earlier CA2225 pass moved several throwing bodies out of `op_Implicit` into named `FromX` factories. CA1065 only inspects "unexpected locations" (`ToString`, operators, `Equals`, `GetHashCode`, static constructors), so the count dropped 7 → 4 **without any behavior change** — the implicit conversions still threw, one frame down. That was a measurement artifact, not progress, and it is recorded here so the 7 → 4 → 0 sequence is not misread later. The operators were subsequently deleted outright ([[implicit-operators-removed-from-predicate-samples]]), which resolved those three honestly.

## Consequence: CA1008

Removing `Error = 0` immediately tripped CA1008 (enums should have a zero value) on all three enums — the analyzer asking for the phantom state back. Suppressed solution-wide in `Directory.Build.props` with a justification.

The reasoning: these enums alias `TokenIds`, where `0` is already `TokenIds.WHITE_SPACE` — not a keyword or an operator. A zero member would be semantically wrong, not merely redundant. Same shape as [[ca1028-conflicts-with-symbol-tokenid]]: a rule fighting a correct design rather than catching debt.

Worth noting kingo reaches the opposite conclusion for its own `ErrorType`, which keeps `Undefined = 0` documented as the `default(Error)` hole "treated as a bug rather than a domain outcome". That reasoning is struct-specific — `default(Error)` is reachable for a value type. Lexi's enums live inside validated `record` classes where `default` is not a reachable path, so the hole CA1008 protects against does not exist here.
