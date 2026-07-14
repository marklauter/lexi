---
title: codeql.yml is still on checkout v4
summary: Every other workflow uses actions/checkout@v7. codeql.yml was deliberately excluded from the modernization because Dependabot PR #22 bumps github/codeql-action 3 to 4 in that file. Once #22 merges, bring its checkout to v7.
tags: [note, todo, lexi, ci, dependencies]
created: 2026-07-14
priority: low
effort: low
status: deferred
---

# codeql.yml is still on checkout v4

`.github/workflows/codeql.yml` uses `actions/checkout@v4` and `github/codeql-action@v3`. Every other workflow in the repo is on `actions/checkout@v7`.

The file was excluded from the modernization on purpose: Dependabot PR **#22** ("Bump github/codeql-action from 3 to 4") already targets it, and touching it would have conflicted. The modernization left it byte-identical, verified.

Un-parks when #22 merges. At that point `codeql-action` is on v4 and the only remaining drift is `checkout@v4` → `@v7`, which is a one-line change.

Note lexi's `codeql.yml` also carries an `actions` + `csharp` language matrix, which pool and plumber share but the six baseline repos do not. That is a separate question from the version pins.
