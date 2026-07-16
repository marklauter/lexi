---
title: Test package refs and InternalsVisibleTo are duplicated across csprojs
summary: The seven-package test stack is repeated verbatim in all three test csprojs and InternalsVisibleTo in four project files. The scaffolding template hoists both into Directory.Build.props behind the .Tests condition. Pool duplicates them too, so this is a canon-vs-template split like ThresholdStat rather than a lexi defect.
tags: [note, todo, lexi, build, canon, house-standard]
created: 2026-07-14
priority: low
effort: low
status: open
---

# Test package refs and InternalsVisibleTo are duplicated across csprojs

Two pieces of per-project boilerplate that the scaffolding-csharp template declares once and lexi repeats.

**The test stack, x3.** This block appears verbatim in `tests/Lexi.Tests/Lexi.Tests.csproj`, `samples/Math/Math.Parser.Tests/Math.Parser.Tests.csproj`, and `samples/Predicate/Predicate.Parser.Tests/Predicate.Parser.Tests.csproj` — seven `PackageReference`s each, with the same `PrivateAssets`/`IncludeAssets` metadata on four of them:

```
Microsoft.Extensions.Options.ConfigurationExtensions
Microsoft.NET.Test.Sdk
xunit.v3
Xunit.DependencyInjection
xunit.runner.visualstudio      (PrivateAssets=all)
coverlet.collector             (PrivateAssets=all)
coverlet.msbuild               (PrivateAssets=all)
```

The template puts this in `Directory.Build.props` under `Condition="$(MSBuildProjectName.EndsWith('.Tests'))"`, alongside `<Using Include="Xunit" />` — which also removes the need for the three `GlobalUsings.cs` files. Plumber follows the template. A fourth test project would mean a fourth copy.

**InternalsVisibleTo, x4.** `src/Lexi/Lexi.csproj`, `samples/Math/Math.Parser/Math.Parser.csproj`, and `samples/Predicate/Predicate.Parser/Predicate.Parser.csproj` each carry `<InternalsVisibleTo Include="$(MSBuildProjectName).Tests" />`. The template declares it once:

```xml
<ItemGroup Condition="!$(MSBuildProjectName.EndsWith('.Tests'))">
  <InternalsVisibleTo Include="$(MSBuildProjectName).Tests" />
</ItemGroup>
```

The `$(MSBuildProjectName)` form is already self-describing, so hoisting is a pure deletion — every site expands to exactly what it says today.

## The tension

writing-csharp names this directly: "One source of truth — Central Package Management, solution-wide compiler flags in `Directory.Build.props` [...] Duplicated knowledge is a defect." By that reading, hoisting is correct.

But the reference repos do not agree with the template:

| repo | test refs | InternalsVisibleTo |
|---|---|---|
| pool | per-csproj | per-csproj |
| dynamodblite | per-csproj | per-csproj |
| plumber | hoisted to Directory.Build.props | per-csproj |
| scaffolding template | hoisted | hoisted |

So this is the same shape of problem as [[thresholdstat-total-vs-minimum-disagreement]]: the template says one thing, the 2-of-3 repo majority says another, and lexi followed the repos. Recorded together because settling either one is the same conversation — whether the template or the installed base is the source of truth when they diverge.

Worth noting the two are not symmetrical. Hoisting `InternalsVisibleTo` is free. Hoisting the test refs is slightly lossy: `Microsoft.Extensions.Options.ConfigurationExtensions` and `Xunit.DependencyInjection` are lexi-specific (the `Startup.cs` DI fixtures) and are not in the template's block, so a hoist either adds them to every future test project or leaves a partial split where some refs are central and some are local. The partial split is arguably worse than either extreme.

No action while the house standard is unsettled. Blocked on the same decision as [[thresholdstat-total-vs-minimum-disagreement]].
