---
title: README badges diverged from canon
summary: The badge block hardcoded the NuGet version, mislabelled the .NET badge as "Nuget", and sat under a "## Build Status" header canon does not have. Adopted the pool/plumber/dynamodblite block, whose NuGet badge is dynamic and self-updates on publish.
tags: [note, lexi, docs, canon, packaging]
created: 2026-07-14
priority: low
effort: low
status: closed
---

# README badges diverged from canon

Canon (pool, plumber, dynamodblite) opens the README with four badges and no header. Lexi had four badges under a `## Build Status` header, diverging in four ways:

1. **The NuGet version was hardcoded** — `img.shields.io/badge/Nuget-v2.2.2-blue`. Canon uses `img.shields.io/nuget/v/<PackageId>?logo=nuget`, which reads the live version off nuget.org.
2. **The .NET badge was labelled `Nuget`** — `[![Nuget](https://img.shields.io/badge/.NET-10.0-blue)]`. A plain bug; the shield rendered `.NET 10.0` while its alt text said `Nuget`. Canon labels it `.NET`.
3. **`## Build Status` header** — canon starts the file with the badges directly. Removed, along with the stray `##` above the logo.
4. **Missing trailing slash** on the dotnet download URL, unlike all three canon repos.

Applied — `README.md:1-5` now matches canon's block exactly, with `MSL.Lexi` substituted:

```markdown
[![NuGet](https://img.shields.io/nuget/v/MSL.Lexi?logo=nuget)](https://www.nuget.org/packages/MSL.Lexi/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/)
```

## Why the dynamic badge matters beyond canon-conformance

The hardcoded shield was a maintenance obligation: every release had to remember to bump it, and it was already stale-by-design in this branch — `v2.2.2` was still *true* while unreleased, and would have silently become a lie the moment `v3.0.0` published. The dynamic badge removes the step rather than deferring it, which is why the badge bump is no longer on the [[lexi-3-0-0-is-a-breaking-release]] release checklist.

## Not adopted

Canon also carries MSL Armory branding under the badges — an `![MSL Armory](.../images/msl.armory.small.png)` image and the tagline *"Another weapon from the MSL Armory"*. Lexi has neither, and `images/` holds only `lexi.png` / `lexi.svg`. Adopting it means copying the asset in, which is a branding decision rather than a badge fix, so it is left alone. Raise separately if lexi should join the armory branding.
