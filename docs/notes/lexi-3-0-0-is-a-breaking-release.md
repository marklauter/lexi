---
title: Next MSL.Lexi release is 3.0.0
summary: MSL.Lexi is published at 2.2.2. The modernization drops net6.0/net7.0/net8.0 for net10.0-only, which forces every consumer to retarget. That is a major bump regardless of any other API change.
tags: [note, todo, lexi, semver, packaging, release]
created: 2026-07-14
priority: high
effort: low
status: open
---

# Next MSL.Lexi release is 3.0.0

`MSL.Lexi` is published on nuget.org at `2.2.2` (versions: 1.0.0, 1.0.1, 1.0.2, 1.0.3, 2.0.0, 2.1.0, 2.1.1, 2.2.0, 2.2.2). Latest release tag is `v2.2.2`, 2024-06-04.

The modernization replaces `<TargetFrameworks>net6.0;net7.0;net8.0</TargetFrameworks>` with `<TargetFramework>net10.0</TargetFramework>`. Every existing consumer must retarget to net10 to resolve the package at all. That is the largest possible breaking change and it sets the version floor at `3.0.0` on its own.

This reframes the other API-break decisions: once the TFM drop is accepted, [[commonpatterns-static-breaks-consumers]] and the `ToString()` change in [[source-tostring-behavior-change]] cost nothing additional. They are already inside a major bump.

The publish workflow derives the version from the release tag and validates it against a semver regex, so shipping this means tagging `v3.0.0`. `README.md:4` still carries a `Nuget-v2.2.2` badge that will need the bump.
