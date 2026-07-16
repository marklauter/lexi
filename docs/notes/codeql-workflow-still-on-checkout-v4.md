---
title: codeql.yml is still on checkout v4
summary: Every other workflow uses actions/checkout@v7. codeql.yml was deliberately excluded from the modernization because Dependabot PR #22 bumps github/codeql-action 3 to 4 in that file. Once #22 merges, bring its checkout to v7.
tags: [note, lexi, ci, dependencies]
created: 2026-07-14
priority: low
effort: low
status: closed
---

# codeql.yml is still on checkout v4

`.github/workflows/codeql.yml` used `actions/checkout@v4` and `github/codeql-action@v3`. Every other workflow in the repo is on `actions/checkout@v7`.

The file was excluded from the modernization on purpose: Dependabot PR **#22** ("Bump github/codeql-action from 3 to 4") already targeted it, and touching it would have conflicted. The modernization left it byte-identical, verified.

## Resolution

PR #22 merged to `main` on 2026-07-14, but this branch (`chore/canonical-editorconfig`) predates that merge, so its `codeql.yml` still carried both the old `checkout@v4` **and** `codeql-action@v3`. Rather than leave it half-stale, all three refs were brought to canon in one edit:

- `actions/checkout@v4` → `@v7`
- `github/codeql-action/init@v3` → `@v4`
- `github/codeql-action/analyze@v3` → `@v4`

This matches what #22 did on `main`, so there is no conflict on merge — the two sides now agree on the codeql-action version, and this branch additionally supplies the `checkout` bump. The `actions` + `csharp` language matrix is untouched (a separate question, noted below).

Note lexi's `codeql.yml` also carries an `actions` + `csharp` language matrix, which pool and plumber share but the six baseline repos do not. That is a separate question from the version pins.
