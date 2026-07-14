---
title: Build gate is red after unsuppressing analyzers
summary: The six suppressed rules were removed from samples/Directory.Build.props and Directory.Build.props on request. dotnet build now fails with 86 analyzer errors — 78 in Predicate.Parser, 6 in Lexi.Tests, 2 in Math.Parser. This is the epic tracking their fix.
tags: [note, todo, epic, lexi, analyzers, build-gate]
created: 2026-07-14
priority: high
effort: high
status: open
---

# Build gate is red after unsuppressing analyzers

Mark's ruling: CA1028/CA1031/CA1062/CA1065/CA1724/CA2225 cannot be suppressed — a failing gate is preferable to a `NoWarn`. Both suppression sites were removed:

- `samples/Directory.Build.props` — the whole six-rule `NoWarn` and its justification block.
- `Directory.Build.props:45` — `CA1028` and `CA1062` dropped from the test-project carve-out, leaving `CA1707;IDE1006;IDE0079;CA1515` (which matches the scaffolding-csharp template's test carve-out).

`dotnet build "Lexi.slnx" -c Debug` now fails with **86 errors** under `TreatWarningsAsErrors`:

| rule | count | shape |
|---|---|---|
| CA2225 | 32 | operator overloads need named alternates |
| CA1062 | 28 | validate arguments of public methods |
| CA1065 | 14 | do not raise exceptions in unexpected locations |
| CA1028 | 8 | enum storage should be Int32 |
| CA1724 | 4 | type name matches namespace |

By project: `Predicate.Parser` 78, `Lexi.Tests` 6, `Math.Parser` 2.

**CA1031 fires zero times.** The removed justification claimed the REPLs' catch-all needed it; there was no violation. The suppression was for a rule that never triggered.

Children:

- [[samples-fail-ca2225-ca1062-ca1065]] — the 74 mechanical ones.
- [[ca1028-conflicts-with-symbol-tokenid]] — 8, conflicts with Lexi's own public API.
- [[ca1724-parser-type-matches-namespace]] — 4, needs a rename.

Status rolls up: `open` while any child is open.
