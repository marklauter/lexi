---
title: Publish pushes to GitHub Packages and pool does not
summary: dotnet.publish.yml pushes to nuget.pkg.github.com as well as nuget.org, and takes packages: write to do it. Pool's canonical shape pushes only to nuget.org. This pre-dates the modernization, so it is a standing divergence rather than a regression.
tags: [note, todo, lexi, ci, packaging, canon]
created: 2026-07-14
priority: low
effort: low
status: open
---

# Publish pushes to GitHub Packages and pool does not

`.github/workflows/dotnet.publish.yml` has a "Publish to nuget.pkg.github.com" step alongside the nuget.org push, and grants `packages: write` for it. Pool's canonical publish workflow pushes only to `api.nuget.org`.

This **pre-dates** the modernization — the parent branch's `dotnet.publish.yml` had the same step. The modernization agent kept it deliberately, on the reasoning that silently dropping a publish target is worse than deviating from pool. That reasoning holds; recording it so the divergence is a decision rather than an accident.

Both push steps are gated `if: github.event_name == 'release'`, so a `workflow_dispatch` dry run packs and uploads an artifact but cannot push to either feed.

The question to settle: is the GitHub Packages mirror still wanted? If yes, this is a permanent, documented deviation from pool and arguably pool should adopt it. If no, drop the step and `packages: write` with it and lexi matches pool exactly.
