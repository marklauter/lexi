---
title: CommonPatterns became static and breaks consumers
summary: src/Lexi/CommonPatterns.cs:8 changed from public partial class to public static partial class to satisfy CA1052. Any consumer with new CommonPatterns() fails to compile with CS0712. Ruling — the break stands, CA1052 is not suppressed.
tags: [note, lexi, api, breaking-change, analyzers]
created: 2026-07-14
priority: low
effort: low
status: closed
---

# CommonPatterns became static and breaks consumers

`src/Lexi/CommonPatterns.cs:8` went from `public partial class CommonPatterns` to `public static partial class CommonPatterns`. This was not requested by the modernization task — CA1052 (static holder types should be static) surfaced it once `AnalysisMode=All` was turned on in `Directory.Build.props:10`, and the agent took the break silently with no justification comment.

Verified by compiling a consumer against the built `Lexi.dll`:

```
error CS0712: Cannot create an instance of the static class 'CommonPatterns'
error CS0723: Cannot declare a variable of static type 'CommonPatterns'
```

Failure scenario: any consumer of `MSL.Lexi` 2.2.2 with `new CommonPatterns()`, `CommonPatterns x = ...`, or `CommonPatterns` as a generic type argument fails to compile on upgrade.

Mitigating: every member on the type is `public static partial Regex` (`NewLine`, `Whitespace`, `IntegerLiteral`, `FloatingPointLiteral`, `ScientificNotationLiteral`, `QuotedStringLiteral`, `CharacterLiteral`, `Identifier`). Instantiating it was always useless. Real blast radius is likely near zero.

The two options were to take the break, or suppress CA1052 with a justification naming binary compatibility.

## Ruling: the break stands

CA1052 is not suppressed and the type stays `static`. The rule is right here — every member is `public static partial Regex` (`NewLine`, `Whitespace`, `IntegerLiteral`, `FloatingPointLiteral`, `ScientificNotationLiteral`, `QuotedStringLiteral`, `CharacterLiteral`, `Identifier`), so an instance could never do anything. A consumer holding one was already writing dead code; the compiler now says so.

This did **not** need the major bump as cover. [[lexi-3-0-0-is-a-breaking-release]] makes the break free, but the change is correct on its own merits and would be the right call at any version — suppressing a rule to preserve the ability to instantiate a type with no instance members would be preserving a defect. The version is settled at release time and is not a concern of this PR.

Ships as a release note under the 3.0.0 breaks.
