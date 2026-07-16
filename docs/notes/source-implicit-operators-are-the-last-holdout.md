---
title: Source's implicit operators are the last holdout
summary: src/Lexi/Source.cs:65,79 still declares implicit string<->Source conversions in the published library. Named alternates already exist beside them, so removal is two lines — but unlike the samples, this one is genuinely used implicitly and removing it breaks lexer.NextMatch("text"). Needs Mark's call.
tags: [note, todo, lexi, api, design, breaking-change]
created: 2026-07-15
priority: medium
effort: low
status: resolved
---

> **Resolved 2026-07-16 via option 2.** Both implicit operators removed from `src/Lexi/Source.cs`; `Lexer` gained a
> `NextMatch(string)` overload so `lexer.NextMatch("text")` still works without the ambient string↔Source hazard.
> Internal lean points fixed: `Lexer.NextMatch` now uses `source.ToString()` and `NextOffset` hoists the text once
> and passes it to `Pattern.Match`. `Source.ToString()`/`FromString` remain as the explicit conversions. Breaking
> change — lands with the `v3.0.0` release tag (version is derived from the GitHub release tag, nothing to bump
> in-repo). See [[lexi-3-0-0-is-a-breaking-release]].

# Source's implicit operators are the last holdout

After [[implicit-operators-removed-from-predicate-samples]], the only implicit operators left in the repo are in the **published library**:

- `src/Lexi/Source.cs:65` — `public static implicit operator string(Source script) => script.text;`
- `src/Lexi/Source.cs:79` — `public static implicit operator Source(string source) => new(source);`

Mark's ruling was that implicit operators hide bugs and are no longer used. The samples now comply. `Source` does not.

## Removal is mechanically trivial

Both already have named alternates beside them, added by the modernization to satisfy CA2225:

- `ToString()` at `Source.cs:72` — the alternate for `Source` → `string`.
- `FromString(string)` at `Source.cs:87` — the alternate for `string` → `Source`.

So the change is deleting two members. Nothing needs writing.

## But this one is really used implicitly

This is the difference from the samples, where every call site already used an explicit cast and the operators were dead ergonomics. Here the conversion is load-bearing:

- `tests/Lexi.Tests/LexiTests.cs:31` — `lexer.NextMatch(source)` where `source` is a `string` parameter.
- `Lexer.NextMatch` overloads only on `Source` (`Lexer.cs:33`) and `MatchResult` (`Lexer.cs:26`) — there is no `string` overload.

So `lexer.NextMatch("1 + 2")` works today **only** because of the `string` → `Source` operator. Removing it forces every consumer to write `lexer.NextMatch(Source.FromString("1 + 2"))`, which is worse at the call site.

## The options

1. **Remove both.** Consistent with the ruling. Breaks `NextMatch("text")` for consumers. Free to take inside [[lexi-3-0-0-is-a-breaking-release]] since that release already forces a retarget.
2. **Remove both, add a `NextMatch(string)` overload.** Keeps the ergonomic, makes the conversion explicit at the one place it is actually wanted, and kills the silent-conversion-anywhere hazard. Costs one overload on `Lexer`. Probably the best of the three.
3. **Keep them.** `Source` is a thin wrapper over `string` and the conversion is unambiguous, so the bug-hiding risk is at its lowest here. Contradicts the ruling.

Note the ruling's own rationale points at option 2: the objection to implicit operators is that they convert *silently, anywhere*, not that `string` → `Source` is a bad idea at the call site where you want it. An explicit overload gives the ergonomic without the ambient hazard.

Needs Mark's decision. Not urgent — the samples were the analyzer problem, and they are fixed.
