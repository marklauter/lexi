---
title: IncludeSymbols is new and 3.0.0 ships a snupkg for the first time
summary: Directory.Build.props adds IncludeSymbols=true solution-wide. Main never set it, so SymbolPackageFormat=snupkg sat inert and pack emitted only a nupkg. From 3.0.0 pack emits a snupkg too, which dotnet nuget push auto-sends alongside the nupkg. Good for nuget.org; it was the reason the GitHub Packages push had to go.
tags: [note, lexi, packaging, ci, behavior-change]
created: 2026-07-14
priority: medium
effort: low
status: closed
---

# IncludeSymbols is new and 3.0.0 ships a snupkg for the first time

`Directory.Build.props:12` sets `<IncludeSymbols>true</IncludeSymbols>` for every project. That line is **new in this branch** — main had no `Directory.Build.props` at all, and `Lexi/Lexi.csproj` on main set `<SymbolPackageFormat>snupkg</SymbolPackageFormat>` without ever setting `IncludeSymbols`. `SymbolPackageFormat` only chooses the *format* of a symbols package; it does not cause one to be produced. So on main the property was inert and `dotnet pack` emitted a single `.nupkg`.

Verified both sides against `src/Lexi/Lexi.csproj` on this branch:

```
dotnet pack ... -o out                          → MSL.Lexi.3.0.0.nupkg + MSL.Lexi.3.0.0.snupkg
dotnet pack ... -o out -p:IncludeSymbols=false  → MSL.Lexi.3.0.0.nupkg
```

The second is main's effective behavior. This is why all nine published releases (1.0.0 through 2.2.2) pushed cleanly with no symbols package in play.

## Why it matters

`dotnet nuget push nuget/*.nupkg` does not need the `.snupkg` named on the command line. The shell glob matches only the `.nupkg`, but the NuGet client detects a matching symbols package sitting beside it and pushes that to the same source unless `--no-symbols` is passed. So adding `IncludeSymbols` silently changed what every push step in `dotnet.publish.yml` sends.

**For api.nuget.org this is an improvement and is the point of the change.** Combined with the SourceLink setup (`PublishRepositoryUrl`, `EmbedUntrackedSources`, and `ContinuousIntegrationBuild` under `GITHUB_ACTIONS`), 3.0.0 is the first release where a consumer can step into Lexi's source in a debugger. Worth calling out in the release notes as a feature.

**For nuget.pkg.github.com it was a problem.** GitHub Packages does not accept symbol packages, so the v3.0.0 tag would have been the first push where that step had a `.snupkg` to reject — a release-tag failure with no prior signal, on a step that had been green for two years.

## Resolution

Mark no longer uses GitHub Packages, so the push step and its `packages: write` permission were removed outright rather than patched with `--no-symbols`. See [[publish-pushes-to-github-packages]]. That resolves the risk and brings the workflow in line with pool, which pushes only to nuget.org.

The generalisable lesson: `SymbolPackageFormat` without `IncludeSymbols` is a no-op, so a repo can carry the setting for years and look configured for symbols while shipping none. Turning on `IncludeSymbols` changes the *push* surface, not just the *pack* output, because the client auto-discovers the sibling package.

Ships in [[lexi-3-0-0-is-a-breaking-release]].
