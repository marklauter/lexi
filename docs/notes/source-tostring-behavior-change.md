---
title: Source.ToString now returns the source text
summary: CA2225 drove adding ToString() and FromString(string) to src/Lexi/Source.cs:67-88. Source.ToString() previously returned "Lexi.Source"; it now returns the source text. Observable change, almost certainly an improvement, but it ships in a published package.
tags: [note, todo, lexi, api, behavior-change]
created: 2026-07-14
priority: low
effort: low
status: open
---

# Source.ToString now returns the source text

`src/Lexi/Source.cs:67-88` gained `ToString()` and `FromString(string)` to satisfy CA2225 (operator overloads have named alternates), which surfaced under the new `AnalysisMode=All`.

The additions themselves are non-breaking. The observable change is that `Source.ToString()` used to return the default `"Lexi.Source"` and now returns the source text.

Failure scenario is narrow: anything that logged, hashed, or asserted on `Source.ToString()`'s output gets different text. Nothing in-repo does. For a type wrapping source text, returning the text is the obviously better behavior, and this ships inside a major bump anyway — see [[lexi-3-0-0-is-a-breaking-release]].

Worth a note only because it is a behavior change in shipped library code that nobody asked for; it arrived as a side effect of turning the analyzers up. Recorded so it lands in the 3.0.0 release notes rather than surprising someone.

Note the inconsistency it creates: the library obeys CA2225 while the samples were suppressing it. That suppression is now removed — see [[samples-fail-ca2225-ca1062-ca1065]] — so the two will be consistent once the samples are fixed.
