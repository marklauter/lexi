---
title: README went stale after the restructure
summary: The samples rename and the net10 retarget left README.md pointing at Samples/ (404 on GitHub) and advertising .NET 6/7/8 badges. README ships inside the nupkg, so both rendered on the nuget.org package page. Fixed.
tags: [note, todo, lexi, docs, packaging]
created: 2026-07-14
priority: high
effort: low
status: closed
---

# README went stale after the restructure

Two independent staleness bugs, both introduced by the modernization and both shipping to nuget.org — `README.md` is packed into `MSL.Lexi.nupkg` via `PackageReadmeFile`, so it renders on the package page as well as the repo front page.

**Dead sample links.** `README.md:25` pointed at `https://github.com/marklauter/lexi/tree/main/Samples/Math` and `.../Samples/Predicate`. The directory is now `samples/` (lowercase) and GitHub tree URLs are case-sensitive, so both would 404 after merge.

**Wrong framework badges.** `README.md:5-7` advertised .NET 6.0, 7.0, and 8.0 — the exact frameworks this change removes.

## Resolution

Both fixed. The three TFM badges collapsed to a single .NET 10.0 badge; the two sample links repointed at `samples/`. Swept the repo for other stale `Samples/` or `net6.0`/`net7.0`/`net8.0` references across `*.md`, `*.yml`, `*.csproj`, `*.props`, and `*.slnx` — none remain.

Still outstanding: `README.md:4` carries a `Nuget-v2.2.2` badge that needs bumping when [[lexi-3-0-0-is-a-breaking-release]] ships.

The generalisable lesson: `Samples/` → `samples/` is a case-only rename, which git recorded correctly (`git ls-tree` confirms lowercase) but which can leave a stale `Samples` directory in existing checkouts on case-insensitive filesystems. Anyone with an old clone may need to re-checkout.
