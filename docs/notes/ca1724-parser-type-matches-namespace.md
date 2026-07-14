---
title: CA1724 needs a Parser type or namespace rename
summary: The type Parser sits in namespaces Math.Parser and Predicate.Parser. CA1724 fires 4 times. The only fixes are renaming the type or the namespace, which renames the sample projects and every reference to them.
tags: [note, todo, lexi, analyzers, samples, naming]
created: 2026-07-14
priority: medium
effort: medium
status: open
blocked_by: "[[build-gate-is-red-after-unsuppressing-analyzers]]"
---

# CA1724 needs a Parser type or namespace rename

4 of the 86 errors:

- `samples/Math/Math.Parser/Parser.cs:9` — type `Parser` conflicts with namespace `Math.Parser`.
- `samples/Predicate/Predicate.Parser/Parser.cs:45` — type `Parser` conflicts with namespace `Predicate.Parser`.

There is no in-place fix. Either the type becomes something like `MathParser` / `PredicateParser`, or the namespace and project become something like `Math.Parsing`. Renaming the namespace renames the project directory and csproj, which touches `Lexi.slnx`, the sample tests, the REPLs, and the `README.md:25` links that were just repointed at `samples/`.

Renaming the type is the smaller change and probably the right one — `Math.Parser.MathParser` is redundant but harmless, and the walkthrough reads the same.

Worth noting the samples' shape is deliberate: `Parser` in `<Sample>.Parser` was chosen to read cleanly in the docs. This is the rule fighting a naming choice rather than catching a defect.
