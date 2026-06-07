---
responsibility:
  owns: project-owner custom rules read by all agents in all phases
  excludes: phase-specific rules (design-only, coding-only)
  delegates_to: custom-design-rules.md (design/design-review), custom-coding-rules.md (impl/impl-review), .asd/rules/ (workflow rules)
---

# Custom Common Rules

## What this project is

OutfitManager is a RimWorld mod that adds work-type score evaluation to the vanilla pawn apparel-scoring algorithm, so pawns pick apparel that better fits the work they actually do. It is a downstream consumer of the shared `LordKuper.Common` library (`..\rimworld-common`). Rules here are inherited from that parent library and adapted to OutfitManager's actual setup; where OutfitManager diverges, the OutfitManager wording wins.

The conventions below are the shared LordKuper RimWorld-mod house standard (same as `rimworld-equipmentmanager` / `rimworld-workmanager`) and are the **target** for OutfitManager. OM is currently still on the legacy stack (see "Project layout / current vs target"); migration to the modern stack is planned project work.

## Project layout (current vs target)

- All source lives under `Source/`.
- **Current (legacy)**: a single production project `Source/OutfitManager.csproj` plus old-format solution `Source/OutfitManager.sln` (MSBuild ToolsVersion 15.0, `packages.config`). Target framework `net48`. References RimWorld `Assembly-CSharp` + Unity modules, `Lib.Harmony` 2.3.6, `Microsoft.CodeAnalysis.NetAnalyzers` 9.0.0, and `LordKuper.Common.dll`. Build output goes to `1.6/Assemblies/`. There is **no test project yet**, no `.slnx`, no `Source/Directory.Build.props`, `Nullable` is **not** enabled.
- **Target (sibling standard)**: `Source/OutfitManager.slnx` + shared `Source/Directory.Build.props` (.NET 10 SDK MSBuild), one folder per project. Production `Source/OutfitManager/` (`net48`, `LangVersion latest`, `Nullable enable`, `Lib.Harmony` 2.4.2 compile-only `PrivateAssets=all`/`ExcludeAssets=runtime`, RimWorld/Unity via `$(RimWorldManagedDir)`, `LordKuper.Common` compile-only `Private=False`, `InternalsVisibleTo` the test project). Tests `Source/OutfitManager.Tests/` (NUnit 4.x + NUnit3TestAdapter + Microsoft.NET.Test.Sdk + **FluentAssertions 7.x**, `net48`, `Nullable enable`).

## Mod identity

- ModId / Harmony id / `About` packageId is `LordKuper.OutfitManager`. Use the `OutfitManagerMod.ModId` constant, never a bare string literal.

## Core behaviour

- The mod's reason to exist is the Harmony transpiler on `JobGiver_OptimizeApparel.ApparelScoreRaw` (`Source/Patches/JobGiverPatch.cs`), which injects `ApparelScoring.GetPawnApparelWorkScore` into vanilla apparel scoring. This patch and the scoring it adds are the integration surface — treat both with care (see `custom-design-rules.md`).

## Upstream dependency

- `LordKuper.Common` is an upstream integration contract, not editable from here. Consume its public surface; do not fork or reimplement what it already provides. Common is resolved at compile time from `$(LordKuperCommonDir)\1.6\Assemblies` (defaults to `..\..\rimworld-common`, overridable via `LORDKUPER_COMMON_DIR`).
- RimWorld build requires `RimWorldManagedDir` / `RIMWORLD_DIR` pointing at RimWorld's `Managed` dir.
