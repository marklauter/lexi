---
title: CommonPatterns became static and breaks consumers
summary: src/Lexi/CommonPatterns.cs:8 changed from public partial class to public static partial class to satisfy CA1052. Any consumer with new CommonPatterns() fails to compile with CS0712. Decide whether to take the break or suppress CA1052.
tags: [note, todo, lexi, api, breaking-change, analyzers]
created: 2026-07-14
priority: high
effort: low
status: open
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

The two options are to take the break, or suppress CA1052 with a justification naming binary compatibility. Taking it is defensible because this release already drops `net6.0`/`net7.0`/`net8.0` — see [[lexi-3-0-0-is-a-breaking-release]], which makes this a rounding error by comparison.
