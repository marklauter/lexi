---
title: Samples fail CA2225, CA1062, and CA1065
summary: 74 of the 86 analyzer errors are named alternates for conversion operators (CA2225 x32), null guards at public boundaries (CA1062 x28), and exceptions thrown from op_Implicit and ToString (CA1065 x14). Mechanical except CA1065, which is a real design flaw.
tags: [note, todo, lexi, analyzers, samples]
created: 2026-07-14
priority: high
effort: high
status: open
blocked_by: "[[build-gate-is-red-after-unsuppressing-analyzers]]"
---

# Samples fail CA2225, CA1062, and CA1065

The bulk of the red gate. Almost all of it is in `samples/Predicate/Predicate.Parser`.

**CA2225 (32) — operator overloads have named alternates.** The AST literal types lean on implicit conversions. Fix is adding named alternates (`ToBoolean`/`FromBoolean` etc.) beside each `op_Implicit`. Mechanical, verbose.

**CA1062 (28) — validate arguments of public methods.** Needs `ArgumentNullException.ThrowIfNull` at the sample parsers' public boundaries. Mechanical.

Note the scaffolding-csharp template turns CA1062 **off** for `-t library` (non-packable) and on for `-t package`, so obeying it in the samples is stricter than canon. That is the intended ruling here, but it is a deliberate deviation from the template.

**CA1065 (14) — do not raise exceptions in unexpected locations.** This one is not ceremony. The analyzer is catching a real flaw:

- `samples/Predicate/Predicate.Parser/Expressions/BooleanLiteral.cs:23` — `op_Implicit` throws `InvalidOperationException`.
- `samples/Predicate/Predicate.Parser/Expressions/ComparisonExpression.cs:21` — `ToString` throws `InvalidOperationException`.
- `samples/Predicate/Predicate.Parser/Expressions/ComparisonOperator.cs:21` — `op_Implicit` throws `ArgumentOutOfRangeException`.
- `samples/Predicate/Predicate.Parser/Expressions/Keyword.cs:17,32,33` — `op_Implicit` and `ToString` throw `ArgumentOutOfRangeException` / `ParseException`.
- `samples/Predicate/Predicate.Parser/Expressions/LogicalExpression.cs:14` — `ToString` throws `InvalidOperationException`.

Throwing from `ToString()` is a genuine bug — a debugger inspecting a malformed AST node crashes. Fixing properly means redesigning the samples' error handling rather than adding a guard clause, so this is the sub-item most likely to need real design thought.
