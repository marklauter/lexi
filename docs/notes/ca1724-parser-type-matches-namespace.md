---
title: CA1724 — Parser type matched its namespace
summary: The type Parser sat in namespaces Math.Parser and Predicate.Parser. Mark chose the namespace rename over the type rename. Both parser projects (dir, csproj, namespace) and their test projects are now Math.Parsing / Predicate.Parsing. This unmasked a real CA1031 in Math.REPL — see below.
tags: [note, lexi, analyzers, samples, naming]
created: 2026-07-14
priority: low
effort: medium
status: closed
blocked_by: "[[build-gate-is-red-after-unsuppressing-analyzers]]"
---

# CA1724 — Parser type matched its namespace

2 errors: `Parser` in `Math.Parser` (`Parser.cs:9`) and `Parser` in `Predicate.Parser` (`Parser.cs:45`). CA1724 flags a type whose name matches its namespace.

## Ruling: rename the namespace, keep the type

Mark chose the namespace rename over renaming the type to `MathParser`/`PredicateParser`. The type stays `Parser` — which reads cleanly in the docs, the reason the samples were shaped this way — and the namespace becomes `Math.Parsing` / `Predicate.Parsing`.

For teaching material the rename was taken all the way, so no `Math.Parser` / `Predicate.Parser` string survives anywhere in the samples, README, or solution:

- Parser projects renamed: directory, `.csproj`, assembly, and all namespaces (`.Parsing`, `.Parsing.Exceptions`, `.Parsing.Expressions`).
- Test projects renamed to match: `Math.Parser.Tests` → `Math.Parsing.Tests` (dir, csproj, namespaces).
- REPL projects kept their names (`Math.REPL`, `Predicate.REPL` — no "Parser" in them); only their `using`s and project references updated.
- `Lexi.slnx` — all four parser/test entries.
- `README.md` — prose references and the code walkthrough.

Done with `git mv` for the directory and csproj renames so history follows, then a scoped string replace for the code. Build, all 57 tests, and `dotnet format --verify-no-changes` are green.

## It unmasked a real CA1031

The rename surfaced a `CA1031` (catch-all `Exception`) in `samples/Math/Math.REPL/Program.cs:32` that had never fired before. Cause: the REPL depends on the parser project, the parser had the CA1724 error, so the REPL never built and its analyzers never ran. Fixing the parser let the REPL compile for the first time under the strict gate.

This **falsifies the epic's earlier claim that "CA1031 fires zero times"** — that measurement was taken while the REPLs could not build. The original samples suppression named the REPL catch-all as the reason for CA1031, and it was right. See [[math-repl-lacked-a-parse-exception-base]] for the fix (Math got the `ParseException` base class Predicate already had, so its REPL now catches the narrow type instead of `Exception` — no suppression needed).
