---
title: An explicit ILLink reference caused the NU1605 restore failure
summary: Lexi.csproj pinned Microsoft.NET.ILLink.Tasks 10.0.9 while net8.0 samples resolved 8.0.28, failing restore with NU1605 and blocking PR #23. The reference was redundant — the SDK injects ILLink — and deleting it fixed the build.
tags: [note, todo, lexi, build, dependencies]
created: 2026-07-14
priority: high
effort: low
status: closed
---

# An explicit ILLink reference caused the NU1605 restore failure

`Lexi/Lexi.csproj:46` carried `<PackageReference Include="Microsoft.NET.ILLink.Tasks" Version="10.0.9" />`. The sample parsers targeted `net8.0` and transitively resolved 8.0.28, so restore failed:

```
error NU1605: Detected package downgrade: Microsoft.NET.ILLink.Tasks from 10.0.9 to 8.0.28
  Predicate.Parser -> MSL.Lexi -> Microsoft.NET.ILLink.Tasks (>= 10.0.9)
  Predicate.Parser -> Microsoft.NET.ILLink.Tasks (>= 8.0.28)
```

This was the failing `test` check on PR #23.

The initial hypothesis — that retargeting to net10 plus Central Package Management would dissolve it — was **wrong**. Retargeting alone leaves the reference pinned at 10.0.9.

## Resolution

Deleted the `Microsoft.NET.ILLink.Tasks` reference outright. The SDK injects ILLink for trimmed publish, so the explicit pin was redundant and was the sole source of the downgrade edge.

Verified no trim analysis was lost: `EnableTrimAnalyzer`, `EnableAotAnalyzer`, and `EnableSingleFileAnalyzer` all remain `true`, supplied by the SDK via `IsTrimmable`/`IsAotCompatible` on net10. `dotnet restore` is clean across all 8 projects from a wiped `obj/`/`bin/`.
