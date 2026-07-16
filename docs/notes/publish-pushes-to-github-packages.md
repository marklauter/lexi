---
title: Publish no longer pushes to GitHub Packages
summary: dotnet.publish.yml pushed to nuget.pkg.github.com alongside nuget.org and took packages: write to do it. Mark no longer uses the feature, so the step and the permission were removed. Lexi now matches pool exactly. This also defused a latent v3.0.0 release failure from the new IncludeSymbols setting.
tags: [note, lexi, ci, packaging, canon]
created: 2026-07-14
priority: low
effort: low
status: closed
---

# Publish no longer pushes to GitHub Packages

`.github/workflows/dotnet.publish.yml` had a "Publish to nuget.pkg.github.com" step alongside the nuget.org push, and granted `packages: write` for it. Pool's canonical publish workflow pushes only to `api.nuget.org`.

The step **pre-dated** the modernization — the parent branch's `dotnet.publish.yml` had the same step. The modernization agent kept it deliberately, on the reasoning that silently dropping a publish target is worse than deviating from pool. That reasoning was right at the time: the divergence needed to be a decision rather than an accident.

## Resolution

Mark's ruling: the GitHub Packages mirror is no longer used. Removed:

- the `Publish to nuget.pkg.github.com` step,
- the `packages: write` entry from the `publish` job's `permissions`, leaving `contents: read`.

`dotnet.publish.yml` now pushes only to `api.nuget.org`, matching pool exactly. The `permissions` block is back to least privilege — nothing in the job needs write access to the package registry any more.

The surviving push is still gated `if: github.event_name == 'release'`, so a `workflow_dispatch` dry run packs and uploads an artifact but cannot push to the feed.

## It was also about to break

Dropping the step retired a latent failure, not just a divergence. This branch adds `IncludeSymbols=true` to `Directory.Build.props`, which main never set, so `dotnet pack` now emits a `.snupkg` beside the `.nupkg` for the first time. `dotnet nuget push nuget/*.nupkg` auto-discovers that sibling and pushes it to the same feed, and GitHub Packages does not accept symbol packages — so the v3.0.0 tag would likely have been the first push where this step had a `.snupkg` to reject, failing a release-tag run that had been green for two years.

Full mechanism and the verified pack output in [[includesymbols-is-new-and-ships-a-snupkg]]. Had the mirror been kept, `--no-symbols` on the GitHub step would have been the fix; dropping the step resolves it outright.

Symbols still publish to nuget.org, which is the desirable half of that change.
