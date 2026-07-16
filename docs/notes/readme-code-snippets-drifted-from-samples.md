---
title: README code snippets drifted from the samples
summary: The README's VocabularyBuilder examples omitted the two .Ignore registrations both samples have, and the Parser listing no longer matched Parser.cs. Synced from source 2026-07-16; the fix is uncommitted on spike/span-first-lexer.
tags: [note, todo, lexi, docs]
created: 2026-07-16
priority: medium
effort: low
status: evolving
---

# README code snippets drifted from the samples

`README.md` quotes three code blocks from the sample projects, and all three had drifted from the files they quote. This is a different staleness than [[readme-went-stale-after-the-restructure]]: the links and badges were fixed there; the quoted code was not compared.

**Missing `.Ignore` registrations.** Both `VocabularyBuilder` examples ended at their last `.Match(...)`. The real `AddParser` methods (`samples/Math/Math.Parsing/ServiceCollectionExtensions.cs`, `samples/Predicate/Predicate.Parsing/ServiceCollectionExtensions.cs`) also register `.Ignore(CommonPatterns.Whitespace(), TokenIds.WHITE_SPACE)` and `.Ignore(CommonPatterns.NewLine(), TokenIds.WHITE_SPACE)`. A reader copying the README example gets a lexer that returns `NoMatch` on the first space. The omission exists on `origin/main` too, so it predates the span-first spike.

**Stale Parser listing.** The "Practical Parser Example" block no longer matched `samples/Math/Math.Parsing/Parser.cs`: member order differed, `Int32.Parse`/`Double.Parse` casing predated the current `int.Parse`/`double.Parse`, and the `// todo: use TryParse` comment was missing.

## Resolution

Synced 2026-07-16: the two `.Ignore` lines added to both examples, and the Parser listing regenerated verbatim from `Parser.cs`. The change sits uncommitted in the working tree on `spike/span-first-lexer`. Left to do: commit it. Also decide whether quoted-code drift deserves a CI check — the README quotes full files, so a diff against the source files would catch the next drift.
