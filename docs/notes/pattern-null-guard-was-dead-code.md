---
title: Pattern's null guard was dead code
summary: src/Lexi/Pattern.cs:70 tested the interpolated string for null instead of the parameter, so a null pattern interpolated to an empty regex matching everywhere. CA1508 surfaced it under AnalysisMode=All. Fixed at the cause.
tags: [note, todo, lexi, bug, api]
created: 2026-07-14
priority: high
effort: low
status: closed
---

# Pattern's null guard was dead code

`src/Lexi/Pattern.cs:70` read:

```csharp
new Regex(@$"\G(?:{pattern})" ?? throw new ArgumentNullException(nameof(pattern)), ...)
```

The `??` bound to the **interpolated string**, which is never null. The guard could never fire. A null `pattern` silently interpolated to `\G(?:)` — a regex matching empty at every position — instead of throwing.

Reachable from the public `Pattern.New(string, uint)` and `Pattern.New(string, uint, RegexOptions)`.

Surfaced by CA1508 (avoid dead conditional code) once `AnalysisMode=All` was enabled in `Directory.Build.props:10`. It had been latent since the code was written.

## Resolution

Guard moved onto the parameter so it throws as intended. Fixed at the cause rather than suppressed — the right call, and the clearest evidence that turning the analyzers up was worth doing.

Only callers who push null past the nullable reference contract see a behavior difference, and they previously got silent wrong matching rather than an exception. Ships in [[lexi-3-0-0-is-a-breaking-release]].
