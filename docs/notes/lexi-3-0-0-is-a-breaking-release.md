---
title: Next MSL.Lexi release is 3.0.0, tagged v3.0.0
summary: The net10.0-only retarget forces a major bump. Versioning is a release-time act, not a PR concern — nothing in the tree carries a version, so there is nothing to do. Kept as the decision record for the major bump and the switch to v-prefixed tags.
tags: [note, lexi, semver, packaging, release]
created: 2026-07-14
priority: low
effort: low
status: closed
---

# Next MSL.Lexi release is 3.0.0, tagged v3.0.0

`MSL.Lexi` is published on nuget.org at `2.2.2` (1.0.0, 1.0.1, 1.0.2, 1.0.3, 2.0.0, 2.1.0, 2.1.1, 2.2.0, 2.2.2).

The modernization replaces `<TargetFrameworks>net6.0;net7.0;net8.0</TargetFrameworks>` with `<TargetFramework>net10.0</TargetFramework>` (`Directory.Build.props:4`). Every existing consumer must retarget to net10 to resolve the package at all. That is the largest possible breaking change and it sets the version floor at `3.0.0` on its own.

## Not this PR's problem

Mark's ruling: the version is decided when the release is cut, not in the PR. Nothing in the working tree needs to change for it — there is no `<Version>` or `<PackageVersion>` property anywhere. `dotnet.publish.yml` derives the version entirely from the release tag and passes it at pack time:

```
if [ "$EVENT" = "release" ]; then version="${TAG#v}"; else version="$INPUT_VERSION"; fi
dotnet pack src/Lexi/Lexi.csproj ... -p:PackageVersion=$PACKAGE_VERSION -p:Version=$PACKAGE_VERSION
```

This also drains the two notes that were deferring to this one. [[commonpatterns-static-breaks-consumers]] and [[source-tostring-behavior-change]] were each waiting to know whether a major bump was available as cover. They did not need it — both changes are correct on their own merits, and are closed as rulings rather than work.

## Tag it `v3.0.0`, with the prefix

The nine existing tags are bare (`1.0.0` … `2.2.2`). Canon is `v`-prefixed: pool `v7.2.0`, dynamodblite `v2.0.1`, plumber `v6.0.0`. Plumber made this exact switch mid-life — its oldest tag is a bare `1.0.50`, and everything from `v1.1.0` on carries the prefix.

So `v3.0.0` is a deliberate convention change, not an inconsistency with lexi's own history. **Do not "fix" the prefix back off to match the old tags.** A major bump is the right place to switch. `${TAG#v}` strips the prefix either way, so the workflow needs no change and the old bare tags remain resolvable.

Title the GitHub release the same as the tag (`v3.0.0`), matching plumber's modern releases rather than its older `Plumber v2.3.2` style.

## Nothing to do

Closed with no action. The breaks that ship under this bump are each recorded in their own note, and the README badge no longer needs bumping — it reads the live version off nuget.org, see [[readme-badges-diverge-from-canon]]. What ships as breaking: the net6/7/8 drop, `CommonPatterns` becoming `static` ([[commonpatterns-static-breaks-consumers]]), `Source.ToString()` returning the source text ([[source-tostring-behavior-change]]), and the first-ever `.snupkg` ([[includesymbols-is-new-and-ships-a-snupkg]]).
